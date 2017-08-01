using System.Collections.Generic;
using Facebook.Spreadsheets.Cells;
using Serilog;

namespace Facebook.Spreadsheets
{
    public partial class Spreadsheet
    {
        private Spreadsheet(ILogger logger)
        {
            SpreadsheetCells = new List<IList<Cell>>();
            Logger = logger;
        }

        internal ILogger Logger { get; }

        private readonly IList<Cell> _cellsToCalculate = new List<Cell>();

        public IList<IList<Cell>> SpreadsheetCells { get; }
    }
}