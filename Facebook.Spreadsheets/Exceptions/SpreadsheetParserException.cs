using System;

namespace Facebook.Spreadsheets.Exceptions
{
    public class SpreadsheetParserException : Exception
    {
        public SpreadsheetParserException(InternalSpreadsheetParserException ex, string currentCellAddress, string cellValue) : base($"There was an error parsing '{cellValue}' in Cell '{currentCellAddress}'. {ex.Message}", ex)
        { }
    }
}