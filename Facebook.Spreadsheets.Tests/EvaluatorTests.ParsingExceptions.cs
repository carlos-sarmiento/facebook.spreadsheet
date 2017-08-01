using Facebook.Spreadsheets.Exceptions;
using Serilog;
using Xunit;

namespace Facebook.Spreadsheets.Tests
{
    public partial class EvaluatorTests
    {
        public void ParsingTestThrows<T>(string testName) where T : InternalSpreadsheetParserException
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
        [InlineData(@"invalidCellCharacters")]
        [InlineData(@"invalidCellCharacters2")]
        [InlineData(@"invalidCellCharacters3")]
        public void InvalidCellCharacters(string testName)
        {
            ParsingTestThrows<InvalidCharacterInCellParsingException>(testName);
        }

        [Theory]
        [InlineData(@"missingCellValue")]
        public void MissingAndInvalidCellValues(string testName)
        {
            ParsingTestThrows<InvalidFormulaParsingException>(testName);
        }

        [Theory]
        [InlineData(@"invalidCellReference")]
        [InlineData(@"invalidCellReference2")]
        [InlineData(@"invalidCellValue")]
        [InlineData(@"invalidFormulaOperand")]
        public void InvalidCellReferencesOrValues(string testName)
        {
            ParsingTestThrows<InvalidValueParsingException>(testName);
        }
    }
}
