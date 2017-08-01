using System;

namespace Facebook.SpreadsheetEvaluation.Exceptions
{
    public class SpreadsheetEvaluationException : Exception
    {
        public SpreadsheetEvaluationException(string message) : base(message)
        { }
    }
}