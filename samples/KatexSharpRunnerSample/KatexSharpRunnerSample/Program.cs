using System;
using AppSoftware.KatexSharpRunner;

namespace KatexSharpRunnerSample
{
    class Program
    {
        // Docs:    Note processing timings
        //          Note required package refs
        //          Note js equivalent library

        static void Main(string[] args)
        {
            // Instantiate KatexJsLatexProcessor. ILatexProcessor provides parsing for raw LaTeX strings.
            // ILatexProcessor can be used standalone or within IMarkdownLatexPreprocessor

            // The argument jintEngineCount causes KatexJsLatexProcessor
            // to create 4 Jint JavaScript Interpreter engine instances and load the KaTeX JS library into
            // each. This is because Jint is not thread safe so KatexJsLatexProcessor manages thread safety 
            // locking during function calls. Multiple instances allows for concurrent processing using a
            // round robin algorithm to distribute requests to each Jint engine.

            // Argument jintEngineLockTimeout specifies a timeout in milliseconds after which an exception will be thrown
            // if a lock cannot be obtained on an instance of the underlying Jint engine if the load on KatexJsLatexProcessor
            // is too high. You can set this value as high as needed.

            var latexProcessor = new KatexJsLatexProcessor(jintEngineCount: 4, jintEngineLockTimeout: 2000);

            // Instantiate MarkdownLatexPreprocessor. IMarkdownLatexPreprocessor provides parsing for LaTeX strings in markdown text.

            var markdownLatexPreprocessor = new MarkdownLatexPreprocessor(latexProcessor);

            // Services can be instantiated via DI as follows

            // services.AddSingleton<ILatexProcessor>(new KatexJsLatexProcessor(4, 2000));
            // services.AddSingleton<IMarkdownLatexPreprocessor, MarkdownLatexPreprocessor>();

            // Render a raw LaTeX expression. 

            string latexHtml = latexProcessor.RenderToStringAsync("f(a,b,c) = (a^2+b^2+c^2)^3").Result;

            Console.WriteLine("Latex expression output:");
            Console.WriteLine(latexHtml);

            string markdown = @"

## Example Markdown

This is an example markdown document

This is an expression in inline delmiters ...

$f(a,b,c) = (a^2+b^2+c^2)^3$

... and this one with block delimiters

$$

\sum_{i=1}^\infty\frac{1}{n^2}=\frac{\pi^2}{6}

$$

";

            // ... or use MarkdownLatexPreprocessor to process LaTeX expressions within
            // markdown text. 

            var markdownHtml = markdownLatexPreprocessor.ProcessLatex(markdown);

            Console.WriteLine("Markdown output:");
            Console.WriteLine(markdownHtml);
            Console.ReadLine();
        }
    }
}
