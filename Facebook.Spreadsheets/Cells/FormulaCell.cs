using Facebook.Spreadsheets.Terms;

namespace Facebook.Spreadsheets.Cells
{
    public class FormulaCell : Cell
    {
        public Term Param1 { get; set; }

        public Term Param2 { get; set; }

        public Operand Operand { get; set; }

        public bool IsBeingEvaluated { get; set; }
    }
}