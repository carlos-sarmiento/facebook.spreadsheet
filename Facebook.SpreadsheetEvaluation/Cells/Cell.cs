namespace Facebook.SpreadsheetEvaluation.Cells
{
    public abstract class Cell
    {
        public decimal? Value { get; set; }

        public string Address { get; set; }
    }
}