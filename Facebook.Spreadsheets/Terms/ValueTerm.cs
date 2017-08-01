using Facebook.Spreadsheets.Exceptions;

namespace Facebook.Spreadsheets.Terms
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
                throw new InvalidValueParsingException(value);
            }

            Value = val;
        }

        public decimal Value { get; }
    }
}