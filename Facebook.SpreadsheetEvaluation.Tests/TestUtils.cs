using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Facebook.SpreadsheetEvaluation.Tests
{
    public static class TestUtils
    {
        public static Stream LoadFileAsStream(string path)
        {
            var assembly = typeof(TestUtils).GetTypeInfo().Assembly;
            var assemblyName = assembly.FullName.Substring(0, assembly.FullName.IndexOf(','));
            var resourceStream = assembly.GetManifestResourceStream($"{assemblyName}.{path.Replace('/', '.').Replace('\\', '.')}");

            if (resourceStream == null)
            {
                throw new FileNotFoundException("File was not found embedded in the Assembly.", path);
            }

            return resourceStream;
        }

        public static string LoadFileAsString(string path)
        {
            var resourceStream = LoadFileAsStream(path);

            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }


        public static void AssertIfOutputEquals(MemoryStream actual, string expected)
        {
            var actualOpen = new MemoryStream(actual.ToArray());


            string actualString;
            using (var reader = new StreamReader(actualOpen, Encoding.UTF8))
            {
                actualString = reader.ReadToEnd();
            }           

            Assert.Equal(expected, actualString);
            Assert.Equal(GetMD5(expected), GetMD5(actualString));
        }

        public static string GetMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                return Convert.ToBase64String(md5.ComputeHash(Encoding.ASCII.GetBytes(input)));
            }
        }
    }
}
