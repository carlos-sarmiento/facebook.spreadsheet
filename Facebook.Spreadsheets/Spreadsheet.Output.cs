using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Facebook.Spreadsheets.Cells;
using Serilog;

namespace Facebook.Spreadsheets
{
    public partial class Spreadsheet
    {
        public void WriteSpreadsheetToStream(Stream outputFileStream)
        {
            if (outputFileStream == null)
            {
                return;
            }

            Logger.Information("Writing to Output");

            using (var output = new StreamWriter(outputFileStream))
            {
                if (SpreadsheetCells.Count == 0)
                {
                    return;
                }

                try
                {
                    for (var i = 0; i < SpreadsheetCells.Count - 1; i++)
                    {
                        WriteRow(SpreadsheetCells[i], output);
                        output.Write("\n");
                    }

                    WriteRow(SpreadsheetCells[SpreadsheetCells.Count - 1], output);
                    output.Flush();

                    Logger.Information("Finished Writing to Output");
                }
                catch (Exception)
                {
                    Log.Logger.Fatal("There was an error writing to the output file.");
                }
            }
        }

        private void WriteRow(IList<Cell> row, StreamWriter output)
        {
            var columnMax = row.Count - 1;

            for (var column = 0; column < columnMax; column++)
            {
                var value = Math.Round(row[column].Value.Value, 9);
                output.Write(value.ToString("0.#########", NumberFormatInfo.InvariantInfo));
                output.Write(',');
            }

            var lastValue = Math.Round(row[columnMax].Value.Value, 9);
            output.Write(lastValue.ToString("0.#########", NumberFormatInfo.InvariantInfo));
        }
    }
}