using System;
using AppSoftware.KatexSharpRunner;

namespace KatexSharpRunnerSample
{
    class Program
    {
        static void Main(string[] args)
        {
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

            var latexProcessor = new KatexJsLatexProcessor(jintEngineCount: 4, jintEngineLockTimeout: 2000);

            var markdownLatexPreprocessor = new MarkdownLatexPreprocessor(latexProcessor);

            var html = markdownLatexPreprocessor.ProcessLatex(markdown);

            Console.WriteLine("Output:");
            Console.WriteLine(html);
            Console.ReadLine();
        }
    }
}
