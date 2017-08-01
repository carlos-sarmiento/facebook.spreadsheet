using System.IO;
using Serilog;
using Xunit;

namespace Facebook.Spreadsheets.Tests
{
    public partial class EvaluatorTests
    {
        static EvaluatorTests()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .CreateLogger();
        }

        [Theory]
        [InlineData(@"emptyFile")]
        [InlineData(@"facebook")]
        [InlineData(@"simple")]
        [InlineData(@"fibonacci")]
        [InlineData(@"pow2Asc")]
        [InlineData(@"pow2Desc")]
        [InlineData(@"negativeMatrixMultiplication")]
        [InlineData(@"negativeMatrixDecimalDivision")]
        [InlineData(@"negativeMatrixMultiplicationLowerCase")]
        [InlineData(@"matrixMultiplication")]
        [InlineData(@"singleReference")]
        [InlineData(@"repeatEvaluation")]
        [InlineData(@"repeatEvaluation2")]
        [InlineData(@"500KCells")]
        public void ValidFiles(string testName)
        {
            var inputFileStream = TestUtils.LoadFileAsStream($"testFiles/valid/{testName}.txt");
            var expectedOutput = TestUtils.LoadFileAsString($"testFiles/valid/{testName}.txt.out");

            var spreadsheetEvaluator = Spreadsheet.LoadSpreadsheetFromStream(inputFileStream, Log.Logger);
            spreadsheetEvaluator.Evaluate();

            var actualOutput = new MemoryStream();

            spreadsheetEvaluator.WriteSpreadsheetToStream(actualOutput);

            TestUtils.AssertIfOutputEquals(actualOutput, expectedOutput);
        }
    }
}
