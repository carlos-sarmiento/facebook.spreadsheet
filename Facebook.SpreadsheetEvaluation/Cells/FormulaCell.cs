using Facebook.SpreadsheetEvaluation.Terms;

namespace Facebook.SpreadsheetEvaluation.Cells
{
    public class FormulaCell : Cell
    {
        public Term Param1 { get; set; }

        public Term Param2 { get; set; }

        public Operand Operand { get; set; }

        public bool IsBeingEvaluated { get; set; }
    }
}