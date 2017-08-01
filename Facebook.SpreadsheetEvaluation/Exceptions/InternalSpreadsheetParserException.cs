using System;

namespace Facebook.SpreadsheetEvaluation.Exceptions
{
    public class InternalSpreadsheetParserException : Exception
    {
        public InternalSpreadsheetParserException(string message) : base(message)
        { }
    }
}