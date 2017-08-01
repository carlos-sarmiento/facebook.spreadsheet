using Facebook.Spreadsheets.Cells;

namespace Facebook.Spreadsheets.Terms
{
    public class OperandTerm : Term
    {
        public OperandTerm(Operand operand)
        {
            Operand = operand;
        }

        public Operand Operand { get; }
    }
}