using System;
using System.Collections.Generic;
using System.Text;
using AppSoftware.KatexSharpRunner.Interfaces;

namespace AppSoftware.KatexSharpRunner
{
    public class MarkdownLatexPreprocessor : IMarkdownLatexPreprocessor
    {
        private readonly ILatexProcessor _latexProcessor;

        public MarkdownLatexPreprocessor(ILatexProcessor latexProcessor)
        {
            _latexProcessor = latexProcessor;
        }

        public string ProcessLatex(string text)
        {
            var latexPositions = GetLatexPositions(text);

            var processedTextStringBuilder = new StringBuilder(text);

            for (int i = latexPositions.Count - 1; i > -1; i--)
            {
                // If latexHtml is an empty string after processing, no replace of delimiters will occur at all

                if (latexPositions[i].LatexString != null)
                {
                    string latexHtml;

                    try
                    {
                        latexHtml = _latexProcessor.RenderToString(latexPositions[i].LatexString);
                    }
                    catch (Exception ex)
                    {
                        latexHtml = "Error in LaTeX '" + latexPositions[i].LatexString + "': " + ex.Message;
                    }

                    // Remove text to be replaced (delimiters plus LaTeX expression)

                    processedTextStringBuilder.Remove(latexPositions[i].OpeningDelimiterStartIndex, latexPositions[i].ClosingDelimiterEndIndex - latexPositions[i].OpeningDelimiterStartIndex + 1);

                    // Replace at position with LaTeX HTML

                    processedTextStringBuilder.Insert(latexPositions[i].OpeningDelimiterStartIndex, latexHtml);
                }
            }

            return processedTextStringBuilder.ToString();
        }

        /// <summary>
        /// Convert LaTeX expressions to HTML in raw markdown along with their positions in string in ascending order.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public IList<LatexPosition> GetLatexPositions(string text)
        {
            // Support $ ... $ delimiters inline, and $$ ... $$ for multiline.

            // https://talk.commonmark.org/t/ignore-latex-like-math-mode-or-parse-it/1926/11

            bool singlelineDelimiterOpen = false;
            bool multiLineDelimiterOpen = false;

            int singlelineOpeningDelimiterStart = -1;
            int singlelineOpeningDelimiterEnd = -1;
            int multilineOpeningDelimiterStart = -1;
            int multilineOpeningDelimiterEnd = -1;

            int endIndex = text.Length - 1;

            var latexStringBuilder = new StringBuilder();

            var latexPositions = new List<LatexPosition>();

            void Terminate()
            {
                string latex = latexStringBuilder.ToString();

                latexStringBuilder.Clear();

                int openingDelimiterStartIndex;
                int closingDelimiterEndIndex;

                if (multilineOpeningDelimiterStart != -1)
                {
                    openingDelimiterStartIndex = multilineOpeningDelimiterStart;
                }
                else if (singlelineOpeningDelimiterStart != -1)
                {
                    openingDelimiterStartIndex = singlelineOpeningDelimiterStart;
                }
                else
                {
                    throw new InvalidOperationException($"{nameof(Terminate)} with no {nameof(openingDelimiterStartIndex)}");
                }

                // Only create LatexPosition if we have delimiter end else this is an invalid termination

                if (multilineOpeningDelimiterEnd != -1 || singlelineOpeningDelimiterEnd != -1)
                {
                    if (singlelineOpeningDelimiterEnd != -1)
                    {
                        closingDelimiterEndIndex = singlelineOpeningDelimiterEnd;
                    }
                    else if (multilineOpeningDelimiterEnd != -1)
                    {
                        closingDelimiterEndIndex = multilineOpeningDelimiterEnd;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Impossible to reach, exception to reassure compiler that {nameof(closingDelimiterEndIndex)} is assigned.");
                    }

                    if (!string.IsNullOrWhiteSpace(latex))
                    {
                        latex = latex.Trim();
                    }
                    else
                    {
                        latex = null;
                    }

                    var latexPosition = new LatexPosition(latex, openingDelimiterStartIndex, closingDelimiterEndIndex);

                    latexPositions.Add(latexPosition);
                }

                // Reset

                singlelineDelimiterOpen = false;
                multiLineDelimiterOpen = false;

                singlelineOpeningDelimiterStart = -1;
                singlelineOpeningDelimiterEnd = -1;
                multilineOpeningDelimiterStart = -1;
                multilineOpeningDelimiterEnd = -1;
            }

            bool inCodeBlock = false;

            for (int i = 0; i < text.Length; i++)
            {
                bool startOfString = i == 0;
                bool eofString = i == endIndex;

                // Don't parse anything while in a code block. The below handles the start and
                // end of the code block as it will allow any number opening backticks and
                // close on the first closing backtick. The subsequent closing backticks have no effect
                // on latex processing.

                if (text[i] == '`' && (startOfString || text[i - 1] != '`'))
                {
                    inCodeBlock = !inCodeBlock;
                }

                if (!inCodeBlock)
                {
                    if (text[i] == '$')
                    {
                        if (!(!startOfString && text[i - 1] == '\\'))
                        {
                            if (!eofString && text[i + 1] == '$')
                            {
                                if (!multiLineDelimiterOpen)
                                {
                                    multilineOpeningDelimiterStart = i;

                                    // Check the content start would not be passed the end of the string and set multilineOpeningContentStart / multiLineDelimiterOpen

                                    if (i + 2 < endIndex)
                                    {
                                        multiLineDelimiterOpen = true;

                                        // Skip to next position as have read the next char (second delimiter)

                                        i++;

                                        // Continue to prevent consumption of delimiter as latex char below

                                        continue;
                                    }
                                }
                                else
                                {
                                    multilineOpeningDelimiterEnd = i + 1;

                                    // Skip to next position as have read the next char (second delimiter)

                                    i++;

                                    Terminate();
                                }
                            }
                            else
                            {
                                if (!multiLineDelimiterOpen)
                                {
                                    if (!singlelineDelimiterOpen)
                                    {
                                        // In single line mode check that char after potential single
                                        // line delimiter is white space so as not to capture $ intended as currency symbol
                                        // e.g. $50

                                        singlelineOpeningDelimiterStart = i;

                                        singlelineDelimiterOpen = true;

                                        // Continue to prevent consumption of delimiter as latex char below

                                        continue;
                                    }
                                    else
                                    {
                                        singlelineOpeningDelimiterEnd = i;

                                        Terminate();
                                    }
                                }
                            }
                        }
                    }
                    else if ((text[i] == '\r') || (text[i] == '\n'))
                    {
                        if (singlelineDelimiterOpen)
                        {
                            Terminate();
                        }
                    }

                    if (singlelineDelimiterOpen || multiLineDelimiterOpen)
                    {
                        // Consume the char

                        latexStringBuilder.Append(text[i]);
                    }
                }
            }

            return latexPositions;
        }
    }
}