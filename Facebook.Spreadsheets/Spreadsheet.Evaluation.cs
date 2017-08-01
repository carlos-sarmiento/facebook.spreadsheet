using System;
using System.Collections.Generic;
using System.Linq;
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

                    var pendingReferences = currentCell.Terms.Where(c => IsReferencePending(c)).Cast<ReferenceTerm>().ToList();

                    if (pendingReferences.Count == 0)
                    {
                        currentCell.Value = CalculatePolishNotation(currentCell.Terms);

                        currentCell.IsBeingEvaluated = false;
                        Logger.Verbose($"Evaluated: {currentCell.Address} = {currentCell.Value}");

                        continue;
                    }

                    evaluationStack.Push(currentCell);

                    foreach (var pendingReference in pendingReferences)
                    {
                        AddCellToStack(evaluationStack, GetCell(pendingReference) as FormulaCell);
                    }
                }
                catch (InternalSpreadsheetEvaluationException ex)
                {
                    throw new SpreadsheetEvaluationException(ex, currentCell.Address, currentCell.Content);
                }
            }
        }

        private decimal GetReferenceValue(ReferenceTerm referenceTerm)
        {
            var cell = GetCell(referenceTerm);
            return cell.Value.Value;
        }

        private bool IsReferencePending(Term param)
        {
            var referenceTerm = param as ReferenceTerm;
            if (referenceTerm != null)
            {
                var cell = GetCell(referenceTerm);
                return !cell.Value.HasValue;
            }

            return false;
        }

        private void AddCellToStack(Stack<FormulaCell> stack, FormulaCell cell)
        {
            if (cell == null || cell.Value.HasValue)
            {
                return;
            }

            if (cell.IsBeingEvaluated)
            {
                throw new CycleDetectedEvaluationException(cell.Address);
            }

            Logger.Verbose($"Queued '{cell.Address}' for evaluation.");

            stack.Push(cell);
        }

        private Cell GetCell(ReferenceTerm reference)
        {
            if (reference.TargetCell != null)
            {
                return reference.TargetCell;
            }

            if (reference.Row >= SpreadsheetCells.Count || reference.Column >= SpreadsheetCells[reference.Row].Count)
            {
                throw new InvalidCellReferenceEvaluationException(reference.Address);
            }

            reference.TargetCell = SpreadsheetCells[reference.Row][reference.Column];

            return reference.TargetCell;
        }

        private decimal CalculatePolishNotation(IEnumerable<Term> terms)
        {
            var evaluationStack = new Stack<decimal>();

            foreach (var term in terms)
            {
                if (term is ValueTerm)
                {
                    evaluationStack.Push(((ValueTerm)term).Value);
                }
                else if (term is ReferenceTerm)
                {
                    evaluationStack.Push(GetReferenceValue((ReferenceTerm)term));
                }
                else if (term is OperandTerm)
                {
                    try
                    {
                        var t2 = evaluationStack.Pop();
                        var t1 = evaluationStack.Pop();

                        var result = CalculateValue(t1, t2, ((OperandTerm)term).Operand);
                        evaluationStack.Push(result);
                    }
                    catch (InvalidOperationException)
                    {
                        throw new InvalidFormulaEvaluationException();
                    }
                }
            }

            if (evaluationStack.Count != 1)
            {
                throw new InvalidFormulaEvaluationException();
            }

            return evaluationStack.Pop();
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