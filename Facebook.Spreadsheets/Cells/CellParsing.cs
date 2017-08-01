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

            if (parts.Length == 1)
            {
                return ParseValueCell(parts[0]);
            }

            if (parts.Length == 3)
            {
                return ParseFormulaCell(parts[2], parts[0], parts[1]);
            }

            throw new InvalidFormulaParsingException();
        }

        private static Cell ParseValueCell(string val)
        {
            var match = CellReferenceRegex.Match(val);

            if (match.Success)
            {
                return new FormulaCell
                {
                    Operand = Operand.Sum,
                    Value = null,
                    IsBeingEvaluated = false,
                    Param1 = new ReferenceTerm(match.Groups["column"].Value, match.Groups["row"].Value),
                    Param2 = new ValueTerm(0)
                };
            }

            return new ValueCell(val);
        }


        private static Cell ParseFormulaCell(string operand, string v1, string v2)
        {
            var op = ParseOperand(operand);

            var matchT1 = CellReferenceRegex.Match(v1);
            var t1 = matchT1.Success ? (Term)new ReferenceTerm(matchT1.Groups["column"].Value, matchT1.Groups["row"].Value) : new ValueTerm(v1);

            var matchT2 = CellReferenceRegex.Match(v2);
            var t2 = matchT2.Success ? (Term)new ReferenceTerm(matchT2.Groups["column"].Value, matchT2.Groups["row"].Value) : new ValueTerm(v2);

            return new FormulaCell()
            {
                Value = null,
                IsBeingEvaluated = false,
                Operand = op,
                Param1 = t1,
                Param2 = t2
            };
        }

        private static Operand ParseOperand(string operand)
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
                    throw new InvalidOperatorParsingException(operand);
            }
        }
    }
}