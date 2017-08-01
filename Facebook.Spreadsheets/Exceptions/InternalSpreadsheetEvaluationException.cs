using System;

namespace Facebook.Spreadsheets.Exceptions
{
    public abstract class InternalSpreadsheetEvaluationException : Exception
    {
        protected InternalSpreadsheetEvaluationException(string message) : base(message) { }
    }

    public class DivisionByZeroEvaluationException : InternalSpreadsheetEvaluationException
    {
        public DivisionByZeroEvaluationException() : base("Division by Zero detected.") { }
    }

    public class CycleDetectedEvaluationException : InternalSpreadsheetEvaluationException
    {
        public CycleDetectedEvaluationException(string cellAddress) : base($"Reference to '{cellAddress}' creates an evaluation cycle.") { }
    }

    public class InvalidCellReferenceEvaluationException : InternalSpreadsheetEvaluationException
    {
        public InvalidCellReferenceEvaluationException(string cellAddress) : base($"Reference to non-existent cell '{cellAddress}'.") { }
    }

    public class OverflowEvaluationException : InternalSpreadsheetEvaluationException
    {
        public OverflowEvaluationException() : base("Operation causes an Overflow/Underflow.") { }
    }
}