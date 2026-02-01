using System;
using System.Globalization;
using Xunit;
using MF.Shared;

namespace mfprobe.Tests
{
    /// <summary>
    /// ValidationMessages クラスのテスト
    /// </summary>
    public class ValidationMessagesTests
    {
        [Fact]
        public void MissingTargetFileName_Japanese_ReturnsJapaneseMessage()
        {
            // Arrange
            var originalCulture = CultureInfo.CurrentUICulture;
            try
            {
                var japaneseCulture = new CultureInfo("ja-JP");
                CultureInfo.CurrentUICulture = japaneseCulture;
                Thread.CurrentThread.CurrentUICulture = japaneseCulture;

                // Act
                string message = ValidationMessages.MissingTargetFileName;

                // Assert
                Assert.Contains("ファイル名", message);
                Assert.Contains("指定", message);
            }
            finally
            {
                CultureInfo.CurrentUICulture = originalCulture;
                Thread.CurrentThread.CurrentUICulture = originalCulture;
            }
        }

        [Fact]
        public void MissingTargetFileName_English_ReturnsEnglishMessage()
        {
            // Arrange
            var originalCulture = CultureInfo.CurrentUICulture;
            try
            {
                var englishCulture = new CultureInfo("en-US");
                CultureInfo.CurrentUICulture = englishCulture;
                Thread.CurrentThread.CurrentUICulture = englishCulture;

                // Act
                string message = ValidationMessages.MissingTargetFileName;

                // Assert
                Assert.Contains("file name", message, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                CultureInfo.CurrentUICulture = originalCulture;
                Thread.CurrentThread.CurrentUICulture = originalCulture;
            }
        }

        [Fact]
        public void ProcessingSuccessful_ReturnsNonEmptyMessage()
        {
            // Act
            string message = ValidationMessages.ProcessingSuccessful;

            // Assert
            Assert.NotNull(message);
            Assert.NotEmpty(message);
        }

        [Fact]
        public void UnexpectedErrorOccurred_ContainsPlaceholder()
        {
            // Act
            string message = ValidationMessages.UnexpectedErrorOccurred;

            // Assert
            Assert.Contains("{0}", message);
        }
    }
}
