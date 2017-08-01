using Facebook.Spreadsheets.Exceptions;
using Serilog;
using Xunit;

namespace Facebook.Spreadsheets.Tests
{
    public partial class EvaluatorTests
    {
        public void EvalTestThrows<T>(string testName) where T : InternalSpreadsheetEvaluationException
        {
            var inputFileStream = TestUtils.LoadFileAsStream($"testFiles/invalidEval/{testName}.txt");

            var exception = Assert.Throws<SpreadsheetEvaluationException>(() =>
            {
                var spreadsheetEvaluator = Spreadsheet.LoadSpreadsheetFromStream(inputFileStream, Log.Logger);
                spreadsheetEvaluator.Evaluate();
            });

            Assert.IsType<T>(exception.InnerException);
        }

        [Theory]
        [InlineData(@"divisionByZero")]
        [InlineData(@"divisionByZeroSimple")]
        public void DivisionByZero(string testName)
        {
            EvalTestThrows<DivisionByZeroEvaluationException>(testName);
        }

        [Theory]
        [InlineData(@"cycleInEval")]
        [InlineData(@"cycleInEvalComplex")]
        public void CycleInEval(string testName)
        {
            EvalTestThrows<CycleDetectedEvaluationException>(testName);
        }

        [Theory]
        [InlineData(@"underflow")]
        [InlineData(@"overflow")]
        public void OverflowInEval(string testName)
        {
            EvalTestThrows<OverflowEvaluationException>(testName);
        }

        [Theory]
        [InlineData(@"invalidCellReference")]
        public void InvalidReferences(string testName)
        {
            EvalTestThrows<InvalidCellReferenceEvaluationException>(testName);
        }

        [Theory]
        [InlineData(@"invalidPolish")]
        [InlineData(@"invalidFormulaOperandHuge")]
        [InlineData(@"missingCellValue2")]
        [InlineData(@"missingCellValue3")]
        [InlineData(@"invalidFormula")]
        public void InvalidFormula(string testName)
        {
            EvalTestThrows<InvalidFormulaEvaluationException>(testName);
        }
    }
}
