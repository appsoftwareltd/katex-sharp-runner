namespace AppSoftware.KatexSharpRunner
{
    public class LatexPosition
    {
        public LatexPosition(string latexString, int openingDelimiterStartIndex, int closingDelimiterEndIndex)
        {
            LatexString = latexString;
            OpeningDelimiterStartIndex = openingDelimiterStartIndex;
            ClosingDelimiterEndIndex = closingDelimiterEndIndex;
        }

        public string LatexString { get; }

        public int OpeningDelimiterStartIndex { get; }

        public int ClosingDelimiterEndIndex { get; }
    }
}