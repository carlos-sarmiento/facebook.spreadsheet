using System;
using System.Collections.Generic;
using Facebook.Spreadsheets.Cells;
using Facebook.Spreadsheets.Exceptions;
using Facebook.Spreadsheets.Terms;

namespace Facebook.Spreadsheets
{
    public partial class Spreadsheet
    {
        public void Evaluate()
        {
            Logger.Information("Starting Evaluation");
            foreach (var cell in _cellsToCalculate)
            {
                EvaluateCell(cell);
            }
            Logger.Information("Evaluation Finished");
        }

        private void EvaluateCell(FormulaCell cell)
        {
            if (cell.Value.HasValue)
            {
                return;
            }

            var evaluationStack = new Stack<FormulaCell>();

            evaluationStack.Push(cell);

            while (evaluationStack.Count > 0)
            {
                var currentCell = evaluationStack.Pop();
                Logger.Verbose($"Evaluating: {currentCell.Address}");

                if (currentCell.Value.HasValue)
                {
                    continue;
                }

                try
                {
                    currentCell.IsBeingEvaluated = true;

                    var (c1, v1) = UnpackReference(currentCell.Param1);
                    var (c2, v2) = UnpackReference(currentCell.Param2);

                    if (v1.HasValue && v2.HasValue)
                    {

                        currentCell.Value = CalculateValue(v1.Value, v2.Value, currentCell.Operand);


                        currentCell.IsBeingEvaluated = false;
                        Logger.Verbose($"Evaluated: {currentCell.Address} = {currentCell.Value}");

                        continue;

                    }

                    evaluationStack.Push(currentCell);

                    AddCellToStack(evaluationStack, c1 as FormulaCell);
                    AddCellToStack(evaluationStack, c2 as FormulaCell);
                }
                catch (InternalSpreadsheetEvaluationException ex)
                {
                    throw new SpreadsheetEvaluationException(ex, currentCell.Address);
                }
            }
        }

        private (Cell cell, decimal? value) UnpackReference(Term param)
        {
            var term = param as ReferenceTerm;
            if (term != null)
            {
                var cell = this.GetCell(term);
                return (cell, cell.Value);
            }

            return (null, ((ValueTerm)param).Value);
        }

        private void AddCellToStack(Stack<FormulaCell> stack, FormulaCell cell)
        {
            if (cell != null && !cell.Value.HasValue)
            {
                if (cell.IsBeingEvaluated)
                {
                    throw new CycleDetectedEvaluationException(cell.Address);
                }

                Logger.Verbose($"Queued '{cell.Address}' for evaluation.");

                stack.Push(cell);
            }
        }

        private Cell GetCell(ReferenceTerm reference)
        {
            if (reference.Row >= SpreadsheetCells.Count || reference.Column >= SpreadsheetCells[reference.Row].Count)
            {
                throw new InvalidCellReferenceEvaluationException(reference.Address);
            }

            return SpreadsheetCells[reference.Row][reference.Column];
        }

        private decimal CalculateValue(decimal v1, decimal v2, Operand op)
        {
            Logger.Verbose($"Calculating: {v1} {v2} {op:G}");

            decimal result;
            try
            {
                switch (op)
                {
                    case Operand.Sum:
                        result = v1 + v2;
                        break;

                    case Operand.Substraction:
                        result = v1 - v2;
                        break;

                    case Operand.Multiplication:
                        result = v1 * v2;
                        break;

                    case Operand.Division:
                        try
                        {
                            result = v1 / v2;
                        }
                        catch (DivideByZeroException)
                        {
                            throw new DivisionByZeroEvaluationException();
                        }
                        break;
                    default:
                        throw new Exception($"Invalid Operation detected {op:G}.");
                }
            }
            catch (OverflowException)
            {
                throw new OverflowEvaluationException();
            }

            Logger.Verbose($"Result: {v1} {v2} {op:G} = {result}");

            return result;
        }
    }
}