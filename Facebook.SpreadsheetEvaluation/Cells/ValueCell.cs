using Facebook.SpreadsheetEvaluation.Exceptions;

namespace Facebook.SpreadsheetEvaluation.Cells
{
    public class ValueCell : Cell
    {
        public ValueCell(string value)
        {
            if (!decimal.TryParse(value, out var val))
            {
                throw new InternalSpreadsheetParserException($"'{value}' is an invalid content for cell.");
            }

            Value = val;
        }

        public ValueCell(decimal value)
        {
            Value = value;
        }
    }
}