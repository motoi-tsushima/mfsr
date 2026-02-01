using System;
using System.IO;
using System.Text;
using Xunit;
using MF.Shared;

namespace MF.Shared.Tests
{
    /// <summary>
    /// EncodingHelper のBOM判定機能のテスト
    /// </summary>
    public class ByteOrderMarkJudgmentTests : IDisposable
    {
        private readonly string _testDirectory;

        public ByteOrderMarkJudgmentTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "mf_shared_tests_" + Guid.NewGuid().ToString());
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
        public void DetectBOM_UTF8WithBOM_ReturnsTrue()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "utf8_bom.txt");
            File.WriteAllText(testFile, "Test", new UTF8Encoding(true));

            // Act
            using (var fs = new FileStream(testFile, FileMode.Open))
            {
                var result = EncodingHelper.DetectOrUseSpecifiedEncoding(
                    fs, testFile, null, MfCommon.EncodingDetectionType.Normal);

                // Assert
                Assert.True(result.BomExists);
            }
        }

        [Fact]
        public void DetectBOM_UTF8WithoutBOM_ReturnsFalse()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "utf8_no_bom.txt");
            File.WriteAllText(testFile, "Test", new UTF8Encoding(false));

            // Act
            using (var fs = new FileStream(testFile, FileMode.Open))
            {
                var result = EncodingHelper.DetectOrUseSpecifiedEncoding(
                    fs, testFile, null, MfCommon.EncodingDetectionType.Normal);

                // Assert
                Assert.False(result.BomExists);
            }
        }

        [Fact]
        public void DetectBOM_UTF16LEWithBOM_ReturnsTrue()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "utf16le_bom.txt");
            File.WriteAllText(testFile, "Test", new UnicodeEncoding(false, true));

            // Act
            using (var fs = new FileStream(testFile, FileMode.Open))
            {
                var result = EncodingHelper.DetectOrUseSpecifiedEncoding(
                    fs, testFile, null, MfCommon.EncodingDetectionType.Normal);

                // Assert
                Assert.True(result.BomExists);
            }
        }

        [Fact]
        public void DetectBOM_UTF16BEWithBOM_ReturnsTrue()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "utf16be_bom.txt");
            File.WriteAllText(testFile, "Test", new UnicodeEncoding(true, true));

            // Act
            using (var fs = new FileStream(testFile, FileMode.Open))
            {
                var result = EncodingHelper.DetectOrUseSpecifiedEncoding(
                    fs, testFile, null, MfCommon.EncodingDetectionType.Normal);

                // Assert
                Assert.True(result.BomExists);
            }
        }

        [Fact]
        public void DetectBOM_EmptyFile_HandlesGracefully()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "empty.txt");
            File.WriteAllText(testFile, "");

            // Act
            using (var fs = new FileStream(testFile, FileMode.Open))
            {
                var result = EncodingHelper.DetectOrUseSpecifiedEncoding(
                    fs, testFile, null, MfCommon.EncodingDetectionType.Normal);

                // Assert - 空ファイルでも正常に処理される
                Assert.NotNull(result);
            }
        }

        [Fact]
        public void DetectBOM_SingleByteFile_ReturnsFalse()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "single.txt");
            File.WriteAllBytes(testFile, new byte[] { 0x41 }); // 'A'

            // Act
            using (var fs = new FileStream(testFile, FileMode.Open))
            {
                var result = EncodingHelper.DetectOrUseSpecifiedEncoding(
                    fs, testFile, null, MfCommon.EncodingDetectionType.Normal);

                // Assert
                Assert.False(result.BomExists);
            }
        }
    }
}
