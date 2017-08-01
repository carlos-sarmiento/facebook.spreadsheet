using System;

namespace Facebook.Spreadsheets.Exceptions
{
    public class SpreadsheetEvaluationException : Exception
    {
        public SpreadsheetEvaluationException(InternalSpreadsheetEvaluationException ex, string currentCellAddress) : base($"An error ocurred evaluating cell '{currentCellAddress}'. {ex.Message}", ex)
        { }
    }
}