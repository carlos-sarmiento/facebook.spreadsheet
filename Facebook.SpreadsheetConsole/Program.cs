using System;
using System.IO;
using Facebook.Spreadsheets;
using Facebook.Spreadsheets.Exceptions;
using Serilog;

namespace Facebook.SpreadsheetConsole
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Information()
                .CreateLogger();

            var fileStreams = OpenFiles(args);

            if (fileStreams == null)
            {
                return;
            }

            var (inputFileStream, outputFileStream) = fileStreams.Value;

            try
            {
                var spreadsheetEvaluator = Spreadsheet.LoadSpreadsheetFromStream(inputFileStream, Log.Logger);
                spreadsheetEvaluator.Evaluate();
                spreadsheetEvaluator.WriteSpreadsheetToStream(outputFileStream.Value);
            }
            catch (SpreadsheetParserException ex)
            {
                Log.Logger.Fatal(ex.Message);
            }
            catch (SpreadsheetEvaluationException ex)
            {
                Log.Logger.Fatal(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal("Unexpected Exception: " + ex.Message);
            }
        }

        private static (FileStream input, Lazy<FileStream> output)? OpenFiles(string[] args)
        {
            if (args.Length != 2)
            {
                Log.Logger.Fatal("Missing Arguments. Application must be called with arguments: <inputFilePath> <outputFilePath>");
                return null;
            }

            var inputFile = args[0];
            var outputFile = args[1];

            var inputFileStream = LoadInputFile(inputFile);
            if (inputFileStream == null)
            {
                return null;
            }

            var outputFileStream = new Lazy<FileStream>(() => OpenOutputFile(outputFile));

            Log.Logger.Information("Input File:  " + inputFile);
            Log.Logger.Information("Output File: " + outputFile);

            return (inputFileStream, outputFileStream);
        }

        private static FileStream LoadInputFile(string inputFile)
        {
            try
            {
                return File.OpenRead(inputFile);
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is ArgumentException)
            {
                Log.Logger.Fatal($"The path: '{inputFile}' is invalid. Please check the path provided.");
            }
            catch (Exception ex) when (ex is PathTooLongException ||
                                       ex is FileNotFoundException ||
                                       ex is DirectoryNotFoundException ||
                                       ex is UnauthorizedAccessException)
            {
                Log.Logger.Fatal($"The file: {inputFile} could not be found. Please check the path provided.");
            }
            catch (Exception)
            {
                Log.Logger.Fatal($"There was an error loading the file: {inputFile}. Please check the path provided.");

            }

            return null;
        }

        private static FileStream OpenOutputFile(string outputFile)
        {
            try
            {
                return new FileStream(outputFile, FileMode.Create, FileAccess.Write);
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is ArgumentException)
            {
                Log.Logger.Fatal($"The path: '{outputFile}' is invalid. Please check the path provided.");
            }
            catch (Exception ex) when (ex is PathTooLongException ||
                                       ex is DirectoryNotFoundException)
            {
                Log.Logger.Fatal($"The file: '{outputFile}' could not be created. Please check the path provided.");
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException)
            {
                Log.Logger.Fatal($"User does not have permission to write to the file: '{outputFile}'.");
            }
            catch (Exception)
            {
                Log.Logger.Fatal($"There was an error creating the file: '{outputFile}'. Please check the path provided.");
            }

            return null;
        }
    }
}