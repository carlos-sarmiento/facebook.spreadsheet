using Facebook.Spreadsheets.Terms;

namespace Facebook.Spreadsheets.Cells
{
    public class FormulaCell : Cell
    {
        public Term[] Terms { get; set; }

        public bool IsBeingEvaluated { get; set; }        
    }
}