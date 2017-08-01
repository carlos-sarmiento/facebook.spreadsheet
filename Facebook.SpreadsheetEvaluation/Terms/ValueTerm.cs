using Facebook.SpreadsheetEvaluation.Exceptions;

namespace Facebook.SpreadsheetEvaluation.Terms
{
    public class ValueTerm : Term
    {
        public ValueTerm(decimal value)
        {
            Value = value;
        }

        public ValueTerm(string value)
        {
            if (!decimal.TryParse(value, out var val))
            {
                throw new InternalSpreadsheetParserException($"'{value}' is an invalid parameter.");
            }

            Value = val;
        }

        public decimal Value { get; }
    }
}