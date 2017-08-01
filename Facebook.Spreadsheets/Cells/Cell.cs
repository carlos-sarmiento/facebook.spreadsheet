namespace Facebook.Spreadsheets.Cells
{
    public abstract class Cell
    {
        public decimal? Value { get; set; }

        public string Address { get; set; }

        public string Content { get; set; }
    }
}