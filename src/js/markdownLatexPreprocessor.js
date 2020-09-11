class MarkdownLatexPreprocessor {

    constructor(latexProcessor) {

        this.latexProcessor = latexProcessor;
    }

    insertAtIndex (str, index, insertStr) {

        if (index > 0)
        {
            return str.substring(0, index) + insertStr + str.substring(index, str.length);
        }

        return insertStr + str;
    }

    processLatex (text) {

        let latexPositions = this.getLatexPositions(text);

        let processedTextStr = text;

        function replaceAt (index, replacement) {
            return this.substr(0, index) + replacement + this.substr(index + replacement.length);
        }

        function insertAtIndex (str, index, insertStr) {

            if (index > 0)
            {
                return str.substring(0, index) + insertStr + str.substring(index, str.length);
            }

            return insertStr + str;
        }

        for (let i = latexPositions.length - 1; i > -1; i--)
        {
            let latexHtml = '';

            // If latexHtml is an empty string after processing, no replace of delimiters will occur at all

            if(latexPositions[i].latexString) {

                try {

                    latexHtml = this.latexProcessor.renderToString(latexPositions[i].latexString);
                }
                catch (e) {

                    if (e instanceof katex.ParseError) {

                        // KaTeX can't parse the expression

                        latexHtml = ("Error in LaTeX '" + latexPositions[i].latexString + "': " + e.message).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
                    } else {

                        // Other error

                        throw e;
                    }
                }

                // Remove text to be replaced (delimiters plus LaTeX expression)

                processedTextStr = processedTextStr.slice(0, latexPositions[i].openingDelimiterStartIndex) + processedTextStr.slice(latexPositions[i].closingDelimiterEndIndex + 1);

                // Replace at position with LaTeX HTML

                processedTextStr = insertAtIndex(processedTextStr, latexPositions[i].openingDelimiterStartIndex, latexHtml);
            }
        }

        return processedTextStr;
    }

    getLatexPositions (text) {

        // Support $ ... $ delimiters inline, and $$ ... $$ for multiline.

        // https://talk.commonmark.org/t/ignore-latex-like-math-mode-or-parse-it/1926/11

        let singlelineDelimiterOpen = false;
        let multiLineDelimiterOpen = false;

        let singlelineOpeningDelimiterStart = -1;
        let singlelineOpeningDelimiterEnd = -1;
        let multilineOpeningDelimiterStart = -1;
        let multilineOpeningDelimiterEnd = -1;

        let endIndex = text.length - 1;

        let latexStr = '';

        let latexPositions = [];

        function terminate()
        {
            let latex = latexStr;

            latexStr = '';

            let openingDelimiterStartIndex;
            let closingDelimiterEndIndex;

            if (multilineOpeningDelimiterStart !== -1)
            {
                openingDelimiterStartIndex = multilineOpeningDelimiterStart;
            }
            else if (singlelineOpeningDelimiterStart !== -1)
            {
                openingDelimiterStartIndex = singlelineOpeningDelimiterStart;
            }
            else
            {
                throw('Terminate with no openingDelimiterStartIndex');
            }

            // Only create LatexPosition if we have delimiter end else this is an invalid termination

            if (multilineOpeningDelimiterEnd !== -1 || singlelineOpeningDelimiterEnd !== -1)
            {
                if (singlelineOpeningDelimiterEnd !== -1)
                {
                    closingDelimiterEndIndex = singlelineOpeningDelimiterEnd;
                }
                else if (multilineOpeningDelimiterEnd !== -1)
                {
                    closingDelimiterEndIndex = multilineOpeningDelimiterEnd;
                }

                if (latex)
                {
                    latex = latex.trim();
                }
                else
                {
                    latex = null;
                }

                let latexPosition = new LatexPosition(latex, openingDelimiterStartIndex, closingDelimiterEndIndex);

                latexPositions.push(latexPosition);
            }

            // Reset

            singlelineDelimiterOpen = false;
            multiLineDelimiterOpen = false;

            singlelineOpeningDelimiterStart = -1;
            singlelineOpeningDelimiterEnd = -1;
            multilineOpeningDelimiterStart = -1;
            multilineOpeningDelimiterEnd = -1;
        }

        let inCodeBlock = false;

        for (let i = 0; i < text.length; i++)
        {
            let startOfString = i === 0;
            let eofString = i === endIndex;

            // Don't parse anything while in a code block. The below handles the start and
            // end of the code block as it will allow any number opening backticks and
            // close on the first closing backtick. The subsequent closing backticks have no effect
            // on latex processing.

            if (text[i] === '`' && (startOfString || text[i - 1] !== '`'))
            {
                inCodeBlock = !inCodeBlock;
            }

            if(!inCodeBlock) {
                if (text[i] === '$') {
                    if (!(!startOfString && text[i - 1] === '\\')) {
                        if (!eofString && text[i + 1] === '$') {
                            if (!multiLineDelimiterOpen) {
                                multilineOpeningDelimiterStart = i;

                                // Check the content start would not be passed the end of the string and set multilineOpeningContentStart / multiLineDelimiterOpen

                                if (i + 2 < endIndex) {
                                    multiLineDelimiterOpen = true;

                                    // Skip to next position as have read the next char (second delimiter)

                                    i++;

                                    // Continue to prevent consumption of delimiter as latex char below

                                    continue;
                                }
                            } else {
                                multilineOpeningDelimiterEnd = i + 1;

                                // Skip to next position as have read the next char (second delimiter)

                                i++;

                                terminate();
                            }
                        } else {
                            if (!multiLineDelimiterOpen) {
                                if (!singlelineDelimiterOpen) {
                                    // In single line mode check that char after potential single
                                    // line delimiter is white space so as not to capture $ intended as currency symbol
                                    // e.g. $50

                                    singlelineOpeningDelimiterStart = i;

                                    singlelineDelimiterOpen = true;

                                    // Continue to prevent consumption of delimiter as latex char below

                                    continue;
                                } else {
                                    singlelineOpeningDelimiterEnd = i;

                                    terminate();
                                }
                            }
                        }
                    }
                } else if ((text[i] === '\r') || (text[i] === '\n')) {
                    if (singlelineDelimiterOpen) {
                        terminate();
                    }
                }

                if (singlelineDelimiterOpen || multiLineDelimiterOpen) {
                    // Consume the char

                    latexStr += text[i];
                }
            }
        }

        return latexPositions;
    }
}

class LatexPosition
{
    latexString = null
    openingDelimiterStartIndex = 0
    closingDelimiterEndIndex = 0

    constructor (latexString, openingDelimiterStartIndex, closingDelimiterEndIndex)
    {
        this.latexString = latexString;
        this.openingDelimiterStartIndex = openingDelimiterStartIndex;
        this.closingDelimiterEndIndex = closingDelimiterEndIndex;
    }
}