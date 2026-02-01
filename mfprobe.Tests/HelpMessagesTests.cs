using System;
using System.Globalization;
using Xunit;
using MF.Shared;

namespace mfprobe.Tests
{
    /// <summary>
    /// HelpMessages クラスのテスト
    /// </summary>
    public class HelpMessagesTests
    {
        [Theory]
        [InlineData("ja-JP", "ja")]
        [InlineData("ja", "ja")]
        [InlineData("ko-KR", "ko")]
        [InlineData("ko", "ko")]
        [InlineData("zh-CN", "zh-CN")]
        [InlineData("zh-Hans", "zh-CN")]
        [InlineData("zh-TW", "zh-TW")]
        [InlineData("zh-Hant", "zh-TW")]
        [InlineData("en-US", "en")]
        [InlineData("en-GB", "en")]
        [InlineData("fr-FR", "en")] // デフォルトは英語
        public void GetLanguageCode_ReturnsCorrectLanguageCode(string cultureName, string expectedLanguageCode)
        {
            // Arrange
            var originalCulture = CultureInfo.CurrentUICulture;
            try
            {
                var culture = new CultureInfo(cultureName);
                CultureInfo.CurrentUICulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                // Act
                string languageCode = HelpMessages.GetLanguageCode();

                // Assert
                Assert.Equal(expectedLanguageCode, languageCode);
            }
            finally
            {
                CultureInfo.CurrentUICulture = originalCulture;
                Thread.CurrentThread.CurrentUICulture = originalCulture;
            }
        }

        [Fact]
        public void GetTxprobeHelpMessage_Japanese_ReturnsJapaneseHelp()
        {
            // Act
            string[] helpMessages = HelpMessages.GetTxprobeHelpMessage("ja");

            // Assert
            Assert.NotNull(helpMessages);
            Assert.NotEmpty(helpMessages);
            Assert.Contains(helpMessages, msg => msg.Contains("ファイル"));
        }

        [Fact]
        public void GetTxprobeHelpMessage_English_ReturnsEnglishHelp()
        {
            // Act
            string[] helpMessages = HelpMessages.GetTxprobeHelpMessage("en");

            // Assert
            Assert.NotNull(helpMessages);
            Assert.NotEmpty(helpMessages);
            Assert.Contains(helpMessages, msg => msg.Contains("file", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void GetRmsmfHelpMessage_ReturnsNonEmptyArray()
        {
            // Act
            string[] helpMessages = HelpMessages.GetRmsmfHelpMessage("en");

            // Assert
            Assert.NotNull(helpMessages);
            Assert.NotEmpty(helpMessages);
        }
    }
}
