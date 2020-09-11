using AppSoftware.KatexSharpRunner.Interfaces;
using Markdig;

namespace AppSoftware.KatexSharpRunner.Tests
{
    public class MarkdownParser
    {
        private readonly IMarkdownLatexPreprocessor _markdownLatexPreprocessor;

        public MarkdownParser(IMarkdownLatexPreprocessor markdownLatexPreprocessor)
        {
            _markdownLatexPreprocessor = markdownLatexPreprocessor;
        }

        public string ParseMarkdownToHtml(string markdown, bool processLatex)
        {
            string html = null;

            if (markdown != null)
            {
                if (processLatex)
                {
                    markdown = _markdownLatexPreprocessor.ProcessLatex(markdown);
                }

                // See https://github.com/lunet-io/markdig

                // NOTE: Match and extend features between markdig (server) and markdown-it
                // where compatible

                var pipeline = new MarkdownPipelineBuilder()
                    .UsePipeTables()                    // As opposed to grid tables?
                    .UseSoftlineBreakAsHardlineBreak()  // \n transforms to <br />
                    .UseAutoLinks()                     // URLs are transformed into hyper links
                    .UseEmphasisExtras()                // strikethrough (and more?)
                    .Build();

                html = Markdown.ToHtml(markdown, pipeline);
            }

            return html;
        }
    }
}