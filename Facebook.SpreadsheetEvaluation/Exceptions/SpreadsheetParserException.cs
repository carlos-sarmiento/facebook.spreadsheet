using System;

namespace Facebook.SpreadsheetEvaluation.Exceptions
{
    public class SpreadsheetParserException : Exception
    {
        public SpreadsheetParserException(string message) : base(message)
        { }
    }
}