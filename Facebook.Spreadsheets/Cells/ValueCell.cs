using Facebook.Spreadsheets.Exceptions;

namespace Facebook.Spreadsheets.Cells
{
    public class ValueCell : Cell
    {
        public ValueCell(string value)
        {
            if (!decimal.TryParse(value, out var val))
            {
                throw new InvalidValueParsingException(value);
            }

            Value = val;
        }       
    }
}