using System.Collections.Generic;

namespace AppSoftware.KatexSharpRunner.Interfaces
{
    public interface IMarkdownLatexPreprocessor
    {
        string ProcessLatex(string text);

        /// <summary>
        /// Convert LaTeX expressions to HTML in raw markdown along with their positions in string in ascending order.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        IList<LatexPosition> GetLatexPositions(string text);
    }
}