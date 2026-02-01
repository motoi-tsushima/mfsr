using System;
using System.IO;
using System.Text;
using Xunit;
using MF.Shared;

namespace mfsr.Tests
{
    /// <summary>
    /// ReplaceStringsInFiles クラスのテスト
    /// </summary>
    public class ReplaceStringsInFilesTests : IDisposable
    {
        private readonly string _testDirectory;

        public ReplaceStringsInFilesTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "mfsr_tests_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Fact]
        public void Replace_SimpleTextReplacement_ReplacesCorrectly()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "test1.txt");
            File.WriteAllText(testFile, "Hello World\nHello C#", Encoding.UTF8);

            string[,] replaceWords = new string[2, 1];
            replaceWords[0, 0] = "Hello";
            replaceWords[1, 0] = "Hi";

            var replacer = new ReplaceStringsInFiles(replaceWords, new[] { testFile }, null);

            // Act
            bool result = replacer.Replace(Encoding.UTF8, Encoding.UTF8, null);

            // Assert
            Assert.True(result);
            string content = File.ReadAllText(testFile, Encoding.UTF8);
            Assert.Contains("Hi World", content);
            Assert.Contains("Hi C#", content);
            Assert.DoesNotContain("Hello", content);
        }

        [Fact]
        public void Replace_MultipleReplacements_ReplacesAllOccurrences()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "test2.txt");
            File.WriteAllText(testFile, "apple banana apple cherry", Encoding.UTF8);

            string[,] replaceWords = new string[2, 2];
            replaceWords[0, 0] = "apple";
            replaceWords[1, 0] = "orange";
            replaceWords[0, 1] = "banana";
            replaceWords[1, 1] = "grape";

            var replacer = new ReplaceStringsInFiles(replaceWords, new[] { testFile }, null);

            // Act
            bool result = replacer.Replace(Encoding.UTF8, Encoding.UTF8, null);

            // Assert
            Assert.True(result);
            string content = File.ReadAllText(testFile, Encoding.UTF8);
            Assert.Contains("orange", content);
            Assert.Contains("grape", content);
            Assert.DoesNotContain("apple", content);
            Assert.DoesNotContain("banana", content);
        }

        [Fact]
        public void Replace_WithBOMOption_AddsOrRemovesBOM()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "test_bom.txt");
            File.WriteAllText(testFile, "Test content", new UTF8Encoding(false));

            string[,] replaceWords = new string[2, 1];
            replaceWords[0, 0] = "Test";
            replaceWords[1, 0] = "Modified";

            var replacer = new ReplaceStringsInFiles(replaceWords, new[] { testFile }, true);

            // Act
            bool result = replacer.Replace(Encoding.UTF8, Encoding.UTF8, null);

            // Assert
            Assert.True(result);
            
            // BOMの確認
            byte[] bytes = File.ReadAllBytes(testFile);
            Assert.True(bytes.Length >= 3);
            Assert.Equal(0xEF, bytes[0]);
            Assert.Equal(0xBB, bytes[1]);
            Assert.Equal(0xBF, bytes[2]);
        }

        [Fact]
        public void Replace_NewLineConversion_CRLF_ConvertsCorrectly()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "test_newline.txt");
            File.WriteAllText(testFile, "Line1\nLine2\nLine3", Encoding.UTF8);

            string[,] replaceWords = new string[2, 0]; // 置換なし

            var replacer = new ReplaceStringsInFiles(replaceWords, new[] { testFile }, null);

            // Act
            bool result = replacer.Replace(Encoding.UTF8, Encoding.UTF8, CommandOptions.NewLineCRLF);

            // Assert
            Assert.True(result);
            string content = File.ReadAllText(testFile);
            Assert.Contains("\r\n", content);
        }

        [Fact]
        public void Replace_NewLineConversion_LF_ConvertsCorrectly()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "test_newline_lf.txt");
            File.WriteAllText(testFile, "Line1\r\nLine2\r\nLine3", Encoding.UTF8);

            string[,] replaceWords = new string[2, 0]; // 置換なし

            var replacer = new ReplaceStringsInFiles(replaceWords, new[] { testFile }, null);

            // Act
            bool result = replacer.Replace(Encoding.UTF8, Encoding.UTF8, CommandOptions.NewLineLF);

            // Assert
            Assert.True(result);
            byte[] bytes = File.ReadAllBytes(testFile);
            string content = Encoding.UTF8.GetString(bytes);
            
            // CRLFがLFに変換されていることを確認
            Assert.DoesNotContain("\r\n", content);
            Assert.Contains("\n", content);
        }

        [Fact]
        public void Replace_NonExistentFile_DoesNotThrowException()
        {
            // Arrange
            string nonExistentFile = Path.Combine(_testDirectory, "non_existent.txt");
            string[,] replaceWords = new string[2, 1];
            replaceWords[0, 0] = "test";
            replaceWords[1, 0] = "replaced";

            var replacer = new ReplaceStringsInFiles(replaceWords, new[] { nonExistentFile }, null);

            // Act & Assert - 例外がスローされないことを確認
            bool result = replacer.Replace(Encoding.UTF8, Encoding.UTF8, null);
            Assert.True(result);
        }

        [Fact]
        public void Replace_MultipleFiles_ReplacesAllFiles()
        {
            // Arrange
            string testFile1 = Path.Combine(_testDirectory, "multi1.txt");
            string testFile2 = Path.Combine(_testDirectory, "multi2.txt");
            File.WriteAllText(testFile1, "Original text", Encoding.UTF8);
            File.WriteAllText(testFile2, "Original data", Encoding.UTF8);

            string[,] replaceWords = new string[2, 1];
            replaceWords[0, 0] = "Original";
            replaceWords[1, 0] = "Modified";

            var replacer = new ReplaceStringsInFiles(
                replaceWords, 
                new[] { testFile1, testFile2 }, 
                null);

            // Act
            bool result = replacer.Replace(Encoding.UTF8, Encoding.UTF8, null);

            // Assert
            Assert.True(result);
            Assert.Contains("Modified", File.ReadAllText(testFile1));
            Assert.Contains("Modified", File.ReadAllText(testFile2));
        }
    }
}
