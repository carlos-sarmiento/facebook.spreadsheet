using Facebook.Spreadsheets.Exceptions;

namespace Facebook.Spreadsheets.Terms
{
    public class ReferenceTerm : Term
    {
        public ReferenceTerm(string column, string row)
        {
            if (!int.TryParse(row, out var rowInt))
            {
                throw new InvalidCellIdentifierParsingException($"{column}{row}");
            }

            Row = rowInt - 1;
            Column = ParseColumn(column, row) - 1;
            Address = $"{column}{row}";
        }

        public string Address { get; set; }

        public int Row { get; set; }

        public int Column { get; set; }

        private static int ParseColumn(string column, string row)
        {
            if (string.IsNullOrWhiteSpace(column))
            {
                throw new InvalidCellIdentifierParsingException($"{column}{row}");
            }

            var sum = 0;

            foreach (var character in column.ToUpperInvariant())
            {
                sum *= 26;
                sum += (character - 'A' + 1);
            }

            return sum;
        }
    }
}