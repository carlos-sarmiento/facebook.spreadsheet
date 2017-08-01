using System;

namespace Facebook.Spreadsheets.Exceptions
{
    public abstract class InternalSpreadsheetParserException : Exception
    {
        protected InternalSpreadsheetParserException(string message) : base(message) { }
    }

    public class InvalidFormulaParsingException : InternalSpreadsheetParserException
    {
        public InvalidFormulaParsingException() : base("Invalid or Incomplete Formula.") { }
    }

    public class InvalidValueParsingException : InternalSpreadsheetParserException
    {
        public InvalidValueParsingException(string value) : base($"'{value}' is not a valid numerical value nor a valid cell reference.") { }
    }

    public class InvalidCellIdentifierParsingException : InternalSpreadsheetParserException
    {
        public InvalidCellIdentifierParsingException(string value) : base($"'{value}' is not a valid cell reference.") { }
    }

    public class InvalidCharacterInCellParsingException : InternalSpreadsheetParserException
    {
        public InvalidCharacterInCellParsingException(char value) : base($"'{value}' is not a valid character in the file.") { }
    }
}