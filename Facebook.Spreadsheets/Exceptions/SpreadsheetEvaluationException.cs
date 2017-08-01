using System;

namespace Facebook.Spreadsheets.Exceptions
{
    public class SpreadsheetEvaluationException : Exception
    {
        public SpreadsheetEvaluationException(InternalSpreadsheetEvaluationException ex, string currentCellAddress, string currentCellContent) : base($"An error ocurred evaluating cell '{currentCellAddress}' with formula '{currentCellContent}'. {ex.Message}", ex) { }
    }
}