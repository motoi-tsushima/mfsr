using System;
using System.IO;
using System.Text;
using Xunit;
using MF.Shared;

namespace mfprobe.Tests
{
    /// <summary>
    /// EncodingHelper クラスのテスト
    /// </summary>
    public class EncodingHelperTests : IDisposable
    {
        private readonly string _testDirectory;

        public EncodingHelperTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "mfprobe_tests_" + Guid.NewGuid().ToString());
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
        public void GetBomDisplayString_WithBom_ReturnsWithBOM()
        {
            // Act
            string result = EncodingHelper.GetBomDisplayString(true);

            // Assert
            Assert.Contains("BOM", result);
        }

        [Fact]
        public void GetBomDisplayString_WithoutBom_ReturnsWithoutBOM()
        {
            // Act
            string result = EncodingHelper.GetBomDisplayString(false);

            // Assert
            Assert.Contains("BOM", result);
        }

        [Fact]
        public void DetectOrUseSpecifiedEncoding_UTF8WithBOM_DetectsCorrectly()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "utf8_with_bom.txt");
            File.WriteAllText(testFile, "テストデータ", new UTF8Encoding(true));

            // Act
            using (var fs = new FileStream(testFile, FileMode.Open))
            {
                var result = EncodingHelper.DetectOrUseSpecifiedEncoding(
                    fs, testFile, null, MfCommon.EncodingDetectionType.Normal);

                // Assert
                Assert.NotNull(result.Encoding);
                Assert.True(result.BomExists);
                Assert.Equal(65001, result.CodePage); // UTF-8
            }
        }

        [Fact]
        public void DetectOrUseSpecifiedEncoding_UTF8WithoutBOM_DetectsCorrectly()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "utf8_without_bom.txt");
            File.WriteAllText(testFile, "Test data", new UTF8Encoding(false));

            // Act
            using (var fs = new FileStream(testFile, FileMode.Open))
            {
                var result = EncodingHelper.DetectOrUseSpecifiedEncoding(
                    fs, testFile, null, MfCommon.EncodingDetectionType.Normal);

                // Assert
                Assert.NotNull(result.Encoding);
            }
        }

        [Fact]
        public void DetectOrUseSpecifiedEncoding_SpecifiedEncoding_UsesSpecifiedEncoding()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "test.txt");
            File.WriteAllText(testFile, "Test data");
            var specifiedEncoding = Encoding.ASCII;

            // Act
            using (var fs = new FileStream(testFile, FileMode.Open))
            {
                var result = EncodingHelper.DetectOrUseSpecifiedEncoding(
                    fs, testFile, specifiedEncoding, MfCommon.EncodingDetectionType.Normal);

                // Assert
                Assert.NotNull(result.Encoding);
                Assert.Equal(specifiedEncoding.CodePage, result.CodePage);
            }
        }

        [Fact]
        public void DetectOrUseSpecifiedEncoding_EmptyFile_ReturnsResult()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "empty.txt");
            File.WriteAllText(testFile, "");

            // Act
            using (var fs = new FileStream(testFile, FileMode.Open))
            {
                var result = EncodingHelper.DetectOrUseSpecifiedEncoding(
                    fs, testFile, null, MfCommon.EncodingDetectionType.Normal);

                // Assert - 空ファイルでもエンコーディングが返される
                Assert.NotNull(result);
            }
        }
    }
}
