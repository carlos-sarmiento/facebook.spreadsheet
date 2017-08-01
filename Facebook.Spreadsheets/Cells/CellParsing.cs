using System;
using System.Text.RegularExpressions;
using Facebook.Spreadsheets.Exceptions;
using Facebook.Spreadsheets.Terms;

namespace Facebook.Spreadsheets.Cells
{
    public static class CellParsing
    {
        private static readonly Regex CellReferenceRegex = new Regex(@"^(?<column>[A-Za-z]+)(?<row>[1-9]\d*)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        public static Cell Parse(string formula)
        {
            var parts = formula.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            Cell cell = null;

            if (parts.Length == 1)
            {
                cell = ParseValueCell(parts[0]);
            }

            else if (parts.Length >= 3)
            {
                cell = ParseFormulaCell(parts);
            }

            if (cell == null)
            {
                throw new InvalidFormulaParsingException();
            }

            cell.Content = formula;

            return cell;
        }

        private static Cell ParseValueCell(string val)
        {
            var match = CellReferenceRegex.Match(val);

            if (match.Success)
            {
                return new FormulaCell
                {
                    Value = null,
                    IsBeingEvaluated = false,
                    Terms = new Term[] { new ReferenceTerm(match.Groups["column"].Value, match.Groups["row"].Value) }
                };
            }

            return new ValueCell(val);
        }


        private static Cell ParseFormulaCell(string[] terms)
        {
            var parsedTerms = new Term[terms.Length];

            for (var i = 0; i < terms.Length; i++)
            {
                var term = terms[i];

                var operand = ParseOperand(term);
                if (operand.HasValue)
                {
                    parsedTerms[i] = new OperandTerm(operand.Value);
                    continue;
                }

                var match = CellReferenceRegex.Match(term);
                parsedTerms[i] = match.Success ? (Term)new ReferenceTerm(match.Groups["column"].Value, match.Groups["row"].Value) : new ValueTerm(term);
            }

            return new FormulaCell()
            {
                Value = null,
                IsBeingEvaluated = false,
                Terms = parsedTerms
            };
        }

        private static Operand? ParseOperand(string operand)
        {
            switch (operand)
            {
                case "+":
                    return Operand.Sum;

                case "-":
                    return Operand.Substraction;

                case "*":
                    return Operand.Multiplication;

                case "/":
                    return Operand.Division;

                default:
                    return null;
            }
        }
    }
}