using System;
using System.IO;
using Serilog;
using Xunit;

namespace Facebook.SpreadsheetEvaluation.Tests
{
    public class EvaluatorTests
    {
        static EvaluatorTests()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Trace()
                .MinimumLevel.Debug()
                .CreateLogger();
        }

        [Theory]
        [InlineData(@"facebook")]
        [InlineData(@"simple")]
        [InlineData(@"fibonacci")]
        [InlineData(@"pow2Asc")]
        [InlineData(@"negativeMatrixMultiplication")]
        [InlineData(@"matrixMultiplication")]
        [InlineData(@"ultraHuge")]
        public void ValidFiles(string testName)
        {
            var inputFileStream = TestUtils.LoadFileAsStream($"testFiles/valid/{testName}.txt");
            var expectedOutput = TestUtils.LoadFileAsString($"testFiles/valid/{testName}.out.txt");

            var spreadsheetEvaluator = Spreadsheet.LoadSpreadsheetFromStream(inputFileStream, Log.Logger);
            spreadsheetEvaluator.Evaluate();

            var actualOutput = new MemoryStream();

            spreadsheetEvaluator.WriteSpreadsheetToStream(actualOutput);

            TestUtils.AssertIfOutputEquals(actualOutput, expectedOutput);
        }

        [Theory]
        [InlineData(@"divisionByZero")]
        public void DivisionByZero(string testName)
        {
            var inputFileStream = TestUtils.LoadFileAsStream($"testFiles/invalid/{testName}.txt");

            var spreadsheetEvaluator = Spreadsheet.LoadSpreadsheetFromStream(inputFileStream, Log.Logger);

            spreadsheetEvaluator.Evaluate();
        }
    }
}
