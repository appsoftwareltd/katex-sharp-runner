using Xunit;
using Xunit.Abstractions;

namespace AppSoftware.KatexSharpRunner.Tests
{
    // https://quicklatex.com/

    // Render test

    // Library output https://codepen.io/appsoftware/pen/abNErge
    // Or pure LaTeX https://quicklatex.com/

    public class MarkdownLatexPreprocessorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public MarkdownLatexPreprocessorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void TestGetLatexPositionsBasic()
        {
            string markdown =
                @"
This is some markdown

## Hello World

$ this is some inline latex $

$$ This is

some multiline latex

$$
";

            var markdownLatexPreprocessor = new MarkdownLatexPreprocessor(new KatexJsLatexProcessor());

            var latexPositions = markdownLatexPreprocessor.GetLatexPositions(markdown);

            Assert.True(latexPositions.Count == 2);

            Assert.True(latexPositions[0].LatexString == "this is some inline latex");

var multilineLatex =

@"This is

some multiline latex";

            Assert.True(latexPositions[1].LatexString == multilineLatex);
        }

        [Fact]
        public void TestGetLatexPositionsBasicNoSpaceBeforeOrAfterDelimiter()
        {
            string markdown =
                @"
This is some markdown

## Hello World

$this is some inline latex$

$$This is

some multiline latex$$
";

            var markdownLatexPreprocessor = new MarkdownLatexPreprocessor(new KatexJsLatexProcessor());

            var latexPositions = markdownLatexPreprocessor.GetLatexPositions(markdown);

            Assert.True(latexPositions.Count == 2);

            Assert.True(latexPositions[0].LatexString == "this is some inline latex");

            var multilineLatex =

                @"This is

some multiline latex";

            Assert.True(latexPositions[1].LatexString == multilineLatex);
        }

        [Fact]
        public void TestEscapeAllDelimiters()
        {
            string markdown =
                @"
This is some markdown

## Hello World

\$this is some inline latex\$

\$\$This is

some multiline latex\$\$
";

            var markdownLatexPreprocessor = new MarkdownLatexPreprocessor(new KatexJsLatexProcessor());

            var latexPositions = markdownLatexPreprocessor.GetLatexPositions(markdown);

            Assert.True(latexPositions.Count == 0);
        }

        [Fact]
        public void TestGetLatexPositionsTextError()
        {
            string markdown =
                @"
This is some markdown

## Hello World

Missing opening delimiter ...

this is some inline latex $

Missing part of closing delimiter ...

$$ This is

some multiline latex

$$ This is

some multiline latex with no terminator


";

            var markdownLatexPreprocessor = new MarkdownLatexPreprocessor(new KatexJsLatexProcessor());

            var latexPositions = markdownLatexPreprocessor.GetLatexPositions(markdown);

            Assert.True(latexPositions.Count == 1);

            var multilineLatex =

                @"This is

some multiline latex";

            Assert.True(latexPositions[0].LatexString == multilineLatex);
        }

        [Fact]
        public void TestProcessLatexBasic()
        {
            string markdown =

                @"
This is some markdown

## Hello World

$ c = \\pm\\sqrt{a^2 + b^2} $

$$

c = \\pm\\sqrt{a^2 + b^2}

$$

## Bye bye world ...

";


            var markdownLatexPreprocessor = new MarkdownLatexPreprocessor(new KatexJsLatexProcessor());

            var latexProcessed = markdownLatexPreprocessor.ProcessLatex(markdown);

            Assert.NotNull(latexProcessed);
        }

        [Fact]
        public void FullMarkdownLatexRenderTest()
        {
            string markdown =
                @"
This is some markdown ...

## Hello world!

$ c = \\pm\\sqrt{a^2 + b^2} $

$$

c = \\pm\\sqrt{a^2 + b^2}

$$

$$E = mc^2$$

### Bye world!

Test empty delimiters 1

$ $

Test empty delimiters 2

$$


$$

";

            var markdownLatexPreprocessor = new MarkdownLatexPreprocessor(new KatexJsLatexProcessor());

            string fullyRenderedMarkdown = new MarkdownParser(markdownLatexPreprocessor).ParseMarkdownToHtml(markdown, true);

            Assert.True(fullyRenderedMarkdown != null);
        }
    }
}