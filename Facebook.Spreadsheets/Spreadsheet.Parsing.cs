using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Facebook.Spreadsheets.Cells;
using Facebook.Spreadsheets.Exceptions;
using Facebook.Spreadsheets.Terms;
using Serilog;

namespace Facebook.Spreadsheets
{
    public partial class Spreadsheet
    {
        public static Spreadsheet LoadSpreadsheetFromStream(Stream inputStream, ILogger logger)
        {
            logger.Information("Parsing Started");
            var spreadsheetEvaluator = new Spreadsheet(logger);

            const int bufferLength = 1024;
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
                                throw new InvalidCharacterInCellParsingException(character);
                            }
                        }
                    }

                    var finalCellValue = stringBuilder.ToString();

                    if (!string.IsNullOrWhiteSpace(finalCellValue))
                    {
                        var finalCell = CellParsing.Parse(finalCellValue);
                        finalCell.Address = $"{GetColumnAsString(currentColumn)}{currentRow}";

                        logger.Verbose($"Parsing { finalCell.Address }: {finalCellValue}");

                        spreadsheetEvaluator.AppendCellToLastRow(finalCell);
                    }

                    else if (string.IsNullOrWhiteSpace(finalCellValue) && currentColumn != 1)
                    {
                        throw new InvalidFormulaParsingException();
                    }

                    else if (string.IsNullOrWhiteSpace(finalCellValue) && currentColumn == 1)
                    {
                        spreadsheetEvaluator.RemoveLastRow();
                    }
                }
            }
            catch (InternalSpreadsheetParserException ex)
            {
                throw new SpreadsheetParserException(ex, $"{GetColumnAsString(currentColumn + 1)}{currentRow + 1}", stringBuilder.ToString());
            }

            logger.Information("Parsing Finished");
            return spreadsheetEvaluator;
        }

        private static string GetColumnAsString(int columnNumber)
        {
            var dividend = columnNumber;
            var columnName = "";

            while (dividend > 0)
            {
                var modulo = (dividend - 1) % 26;
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
                    // Is Number || Is Capital A-Z || Is LowerCase a-z
                    return (c >= 48 && c <= 57) || (c >= 65 && c <= 90) || (c >= 97 && c <= 122);
            }
        }

        private void AddNewRow()
        {
            SpreadsheetCells.Add(new List<Cell>());
        }

        private void RemoveLastRow()
        {
            if (SpreadsheetCells.Count > 0 && SpreadsheetCells[SpreadsheetCells.Count - 1].Count == 0)
            {
                SpreadsheetCells.RemoveAt(SpreadsheetCells.Count - 1);
            }
            else
            {
                throw new Exception("Tried to remove a row that has cells.");
            }
        }

        private void AppendCellToLastRow(Cell cell)
        {
            var formulaCell = cell as FormulaCell;
            if (formulaCell != null)
            {

                _cellsToCalculate.Add(formulaCell);
            }

            SpreadsheetCells[SpreadsheetCells.Count - 1].Add(cell);
        }
    }
}