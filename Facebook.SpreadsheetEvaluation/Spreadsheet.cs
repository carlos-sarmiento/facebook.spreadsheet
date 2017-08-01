using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Facebook.SpreadsheetEvaluation.Cells;
using Facebook.SpreadsheetEvaluation.Exceptions;
using Facebook.SpreadsheetEvaluation.Terms;
using Serilog;

namespace Facebook.SpreadsheetEvaluation
{
    public class Spreadsheet
    {
        private Spreadsheet(ILogger logger)
        {
            SpreadsheetCells = new List<IList<Cell>>();
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        private readonly IList<FormulaCell> _cellsToCalculate = new List<FormulaCell>();

        public IList<IList<Cell>> SpreadsheetCells { get; set; }

        #region Parsing

        public static Spreadsheet LoadSpreadsheetFromStream(Stream inputStream, ILogger logger)
        {
            logger.Information("Parsing Started");
            var spreadsheetEvaluator = new Spreadsheet(logger);

            const int bufferLength = 256;
            var buffer = new char[bufferLength];

            var currentRow = 1;
            var currentColumn = 1;
            var stringBuilder = new StringBuilder();

            try
            {
                using (var input = new StreamReader(inputStream))
                {
                    spreadsheetEvaluator.AddNewRow();
                    while (!input.EndOfStream)
                    {
                        var readCount = input.Read(buffer, 0, bufferLength);

                        for (var i = 0; i < readCount; i++)
                        {
                            var character = buffer[i];

                            if (character == ',' || character == '\n')
                            {
                                var cellValue = stringBuilder.ToString();
                                var address = $"{GetColumnAsString(currentColumn)}{currentRow}";
                                logger.Verbose($"Parsing {address}: {cellValue}");

                                var cell = CellParsing.Parse(cellValue);
                                cell.Address = address;
                                spreadsheetEvaluator.AppendCellToLastRow(cell);

                                stringBuilder = new StringBuilder();
                                currentColumn++;

                                if (character == '\n')
                                {
                                    spreadsheetEvaluator.AddNewRow();
                                    currentColumn = 1;
                                    currentRow++;
                                }
                            }
                            else if (IsValidCharacter(character))
                            {
                                if (character != '\r')
                                {
                                    stringBuilder.Append(character);
                                }
                            }
                            else
                            {
                                throw new InternalSpreadsheetParserException($"Invalid character '{character}' was detected");
                            }
                        }
                    }
                    var finalCellValue = stringBuilder.ToString();
                    var finalCell = CellParsing.Parse(finalCellValue);
                    finalCell.Address = $"{GetColumnAsString(currentColumn)}{currentRow}";

                    logger.Verbose($"Parsing { finalCell.Address }: {finalCellValue}");

                    spreadsheetEvaluator.AppendCellToLastRow(finalCell);
                }
            }
            catch (IOException)
            {
                throw new SpreadsheetParserException("There was an error while reading the input file.");
            }
            catch (InternalSpreadsheetParserException ex)
            {
                throw new SpreadsheetParserException($"There was an error parsing '{stringBuilder}' in Cell '{GetColumnAsString(currentColumn + 1)}{currentRow + 1}'. {ex.Message}");
            }

            logger.Information("Parsing Finished");
            return spreadsheetEvaluator;
        }

        private static string GetColumnAsString(int columnNumber)
        {
            var dividend = columnNumber;
            var columnName = "";
            var modulo = 0;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        private static bool IsValidCharacter(char c)
        {
            switch (c)
            {
                case ' ':
                case '.':
                case '*':
                case '+':
                case '-':
                case '/':
                case '\r':
                    return true;

                default:
                    // Is Number || Is Capital A-Z
                    return (c >= 48 && c <= 57) || (c >= 65 && c <= 90);
            }
        }

        private void AddNewRow()
        {
            SpreadsheetCells.Add(new List<Cell>());
        }

        private void AppendCellToLastRow(Cell cell)
        {
            var formulaCell = cell as FormulaCell;
            if (formulaCell != null)
            {
                var valueTerm1 = formulaCell.Param1 as ValueTerm;
                var valueTerm2 = formulaCell.Param2 as ValueTerm;
                if (valueTerm1 != null && valueTerm2 != null)
                {
                    formulaCell.Value = CalculateValue(valueTerm1.Value, valueTerm2.Value, formulaCell.Operand);
                    Logger.Verbose($"Evaluated: {formulaCell.Address} = {formulaCell.Value}");
                }
                else
                {
                    _cellsToCalculate.Add(formulaCell);
                }
            }

            SpreadsheetCells[SpreadsheetCells.Count - 1].Add(cell);
        }

        #endregion

        #region Evaluation

        public void Evaluate()
        {
            Logger.Information("Starting Evaluation");
            foreach (var cell in _cellsToCalculate)
            {
                try
                {
                    EvaluateCell(cell);
                }
                catch (InternalSpreadsheetEvaluationException ex)
                {
                    throw new SpreadsheetEvaluationException($"An error ocurred evaluating '{cell.Address}'. {ex.Message}");
                }
            }
            Logger.Information("Evaluation Finished");
        }

        private void EvaluateCell(FormulaCell cell)
        {
            if (cell.Value.HasValue)
            {
                return;
            }

            var evaluationStack = new Stack<FormulaCell>();

            evaluationStack.Push(cell);

            while (evaluationStack.Count > 0)
            {
                var currentCell = evaluationStack.Pop();
                Logger.Verbose($"Evaluating: {currentCell.Address}");

                if (currentCell.Value.HasValue)
                {
                    continue;
                }

                currentCell.IsBeingEvaluated = true;

                var (c1, v1) = UnpackReference(currentCell.Param1);
                var (c2, v2) = UnpackReference(currentCell.Param2);

                if (v1.HasValue && v2.HasValue)
                {
                    currentCell.Value = CalculateValue(v1.Value, v2.Value, currentCell.Operand);
                    currentCell.IsBeingEvaluated = false;
                    Logger.Verbose($"Evaluated: {currentCell.Address} = {currentCell.Value}");

                    continue;
                }

                evaluationStack.Push(currentCell);

                AddCellToStack(evaluationStack, c1 as FormulaCell);
                AddCellToStack(evaluationStack, c2 as FormulaCell);
            }
        }

        private (Cell cell, decimal? value) UnpackReference(Term param)
        {
            var term = param as ReferenceTerm;
            if (term != null)
            {
                var cell = this.GetCell(term);
                return (cell, cell.Value);
            }

            return (null, ((ValueTerm)param).Value);
        }

        private void AddCellToStack(Stack<FormulaCell> stack, FormulaCell cell)
        {
            if (cell != null && !cell.Value.HasValue)
            {
                if (cell.IsBeingEvaluated)
                {
                    throw new InternalSpreadsheetEvaluationException($"Reference to '{cell.Address}' creates an evaluation cycle.");
                }

                Logger.Verbose($"Queued '{cell.Address}' for evaluation.");

                stack.Push(cell);
            }
        }

        private Cell GetCell(ReferenceTerm reference)
        {
            if (reference.Row >= SpreadsheetCells.Count || reference.Column >= SpreadsheetCells[reference.Row].Count)
            {
                throw new InternalSpreadsheetEvaluationException($"Cell '{reference.Address}' doesn't exists.");
            }

            return SpreadsheetCells[reference.Row][reference.Column];
        }

        private decimal CalculateValue(decimal v1, decimal v2, Operand op)
        {
            Logger.Verbose($"Calculating: {v1} {v2} {op:G}");

            decimal result;
            try
            {
                switch (op)
                {
                    case Operand.Sum:
                        result = v1 + v2;
                        break;

                    case Operand.Substraction:
                        result = v1 - v2;
                        break;

                    case Operand.Multiplication:
                        result = v1 * v2;
                        break;

                    case Operand.Division:
                        try
                        {
                            result = v1 / v2;
                        }
                        catch (DivideByZeroException)
                        {
                            throw new InternalSpreadsheetEvaluationException("Division by Zero detected.");
                        }
                        break;
                    default:
                        throw new InternalSpreadsheetEvaluationException($"Invalid Operation detected {op:G}.");
                }
            }
            catch (OverflowException)
            {
                throw new InternalSpreadsheetEvaluationException("An Overflow/Underflow error ocurred.");
            }

            Logger.Verbose($"Result: {v1} {v2} {op:G} = {result}");

            return result;
        }

        #endregion

        #region Output

        public void WriteSpreadsheetToStream(Stream outputFileStream)
        {
            using (var output = new StreamWriter(outputFileStream))
            {
                try
                {
                    for (var i = 0; i < SpreadsheetCells.Count - 1; i++)
                    {
                        WriteRow(SpreadsheetCells[i], output);
                        output.WriteLine();
                    }

                    WriteRow(SpreadsheetCells[SpreadsheetCells.Count - 1], output);
                    output.Flush();
                }
                catch (Exception)
                {
                    Log.Logger.Fatal($"There was an error writing to the output file.");
                }
            }
        }

        private void WriteRow(IList<Cell> row, StreamWriter output)
        {
            var columnMax = row.Count - 1;
            for (var column = 0; column < columnMax; column++)
            {
                output.Write(row[column].Value.Value.ToString("0.##############", NumberFormatInfo.InvariantInfo));
                output.Write(',');
            }
            output.Write(row[columnMax].Value.Value.ToString("0.##############", NumberFormatInfo.InvariantInfo));
        }

        #endregion
    }
}