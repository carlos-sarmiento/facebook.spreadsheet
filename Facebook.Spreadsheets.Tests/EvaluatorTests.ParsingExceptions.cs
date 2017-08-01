using Facebook.Spreadsheets.Exceptions;
using Serilog;
using Xunit;

namespace Facebook.Spreadsheets.Tests
{
    public partial class EvaluatorTests
    {
        public void TestThrows<T>(string testName) where T : InternalSpreadsheetParserException
        {
            var inputFileStream = TestUtils.LoadFileAsStream($"testFiles/invalidParsing/{testName}.txt");

            var exception = Assert.Throws<SpreadsheetParserException>(() =>
            {
                var spreadsheetEvaluator = Spreadsheet.LoadSpreadsheetFromStream(inputFileStream, Log.Logger);
                spreadsheetEvaluator.Evaluate();
            });

            Assert.IsType<T>(exception.InnerException);
        }

        [Theory]
        [InlineData(@"invalidFormulaOperand")]
        [InlineData(@"invalidFormulaOperandHuge")]
        public void InvalidFormulaOperand(string testName)
        {
            TestThrows<InvalidOperatorParsingException>(testName);
        }

        [Theory]
        [InlineData(@"invalidCellCharacters")]
        [InlineData(@"invalidCellCharacters2")]
        [InlineData(@"invalidCellCharacters3")]
        public void InvalidCellCharacters(string testName)
        {
            TestThrows<InvalidCharacterInCellParsingException>(testName);
        }

        [Theory]
        [InlineData(@"missingCellValue")]
        [InlineData(@"missingCellValue2")]
        [InlineData(@"missingCellValue3")]
        [InlineData(@"invalidFormula")]
        public void MissingAndInvalidCellValues(string testName)
        {
            TestThrows<InvalidFormulaParsingException>(testName);
        }

        [Theory]
        [InlineData(@"invalidCellReference")]
        [InlineData(@"invalidCellReference2")]
        [InlineData(@"invalidCellValue")]
        public void InvalidCellReferencesOrValues(string testName)
        {
            TestThrows<InvalidValueParsingException>(testName);
        }
    }
}
