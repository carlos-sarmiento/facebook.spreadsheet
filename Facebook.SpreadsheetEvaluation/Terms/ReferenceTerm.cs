using System.Net.Sockets;
using Facebook.SpreadsheetEvaluation.Exceptions;

namespace Facebook.SpreadsheetEvaluation.Terms
{
    public class ReferenceTerm : Term
    {
        public ReferenceTerm(string column, string row)
        {
            if (!int.TryParse(row, out var rowInt))
            {
                throw new InternalSpreadsheetParserException($"{row} is an invalid row identifier.");
            }

            Row = rowInt - 1;
            Column = ParseColumn(column) - 1;
            Address = $"{column}{row}";
        }

        public string Address { get; set; }

        public int Row { get; set; }

        public int Column { get; set; }

        private static int ParseColumn(string column)
        {
            if (string.IsNullOrWhiteSpace(column))
            {
                throw new InternalSpreadsheetParserException($"{column} is an invalid column identifier.");
            }

            var sum = 0;

            foreach (var character in column)
            {
                sum *= 26;
                sum += (character - 'A' + 1);
            }

            return sum;
        }
    }
}