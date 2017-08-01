using Facebook.Spreadsheets.Exceptions;
using Serilog;
using Xunit;

namespace Facebook.Spreadsheets.Tests
{
    public partial class EvaluatorTests
    {
        [Theory]
        [InlineData(@"divisionByZero")]
        [InlineData(@"divisionByZeroSimple")]
        public void DivisionByZero(string testName)
        {
            var inputFileStream = TestUtils.LoadFileAsStream($"testFiles/invalidEval/{testName}.txt");

            var exception = Assert.Throws<SpreadsheetEvaluationException>(() =>
            {
                var spreadsheetEvaluator = Spreadsheet.LoadSpreadsheetFromStream(inputFileStream, Log.Logger);
                spreadsheetEvaluator.Evaluate();
            });

            Assert.True(exception.InnerException is DivisionByZeroEvaluationException);
        }

        [Theory]
        [InlineData(@"cycleInEval")]
        [InlineData(@"cycleInEvalComplex")]
        public void CycleInEval(string testName)
        {
            var inputFileStream = TestUtils.LoadFileAsStream($"testFiles/invalidEval/{testName}.txt");

            var spreadsheetEvaluator = Spreadsheet.LoadSpreadsheetFromStream(inputFileStream, Log.Logger);

            var exception = Assert.Throws<SpreadsheetEvaluationException>(() =>
            {
                spreadsheetEvaluator.Evaluate();
            });

            Assert.True(exception.InnerException is CycleDetectedEvaluationException);
        }

        [Theory]
        [InlineData(@"underflow")]
        [InlineData(@"overflow")]
        public void OverflowInEval(string testName)
        {
            var inputFileStream = TestUtils.LoadFileAsStream($"testFiles/invalidEval/{testName}.txt");

            var spreadsheetEvaluator = Spreadsheet.LoadSpreadsheetFromStream(inputFileStream, Log.Logger);

            var exception = Assert.Throws<SpreadsheetEvaluationException>(() =>
            {
                spreadsheetEvaluator.Evaluate();
            });

            Assert.True(exception.InnerException is OverflowEvaluationException);
        }

        [Theory]
        [InlineData(@"invalidCellReference")]
        public void InvalidReferences(string testName)
        {
            var inputFileStream = TestUtils.LoadFileAsStream($"testFiles/invalidEval/{testName}.txt");

            var spreadsheetEvaluator = Spreadsheet.LoadSpreadsheetFromStream(inputFileStream, Log.Logger);

            var exception = Assert.Throws<SpreadsheetEvaluationException>(() =>
            {
                spreadsheetEvaluator.Evaluate();
            });

            Assert.True(exception.InnerException is InvalidCellReferenceEvaluationException);
        }
    }
}
