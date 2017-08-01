using Facebook.Spreadsheets.Terms;

namespace Facebook.Spreadsheets.Cells
{
    public class Cell
    {
        public decimal? Value { get; set; }

        public string Address { get; set; }

        public string Content { get; set; }

        public Term[] Terms { get; set; }

        public bool IsBeingEvaluated { get; set; }
    }
}