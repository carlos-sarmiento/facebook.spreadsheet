using System;

namespace Facebook.SpreadsheetEvaluation.Exceptions
{
    public class InternalSpreadsheetEvaluationException : Exception
    {
        public InternalSpreadsheetEvaluationException(string message) : base(message)
        {
        }
    }
}