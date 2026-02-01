using System;
using Xunit;
using MF.Shared;

namespace mfsr.Tests
{
    /// <summary>
    /// ValidationMessages の共通テスト（mfsr プロジェクト用）
    /// </summary>
    public class ValidationMessagesTests
    {
        [Fact]
        public void ProcessingSuccessful_ReturnsValidMessage()
        {
            // Act
            string message = ValidationMessages.ProcessingSuccessful;

            // Assert
            Assert.NotNull(message);
            Assert.NotEmpty(message);
        }

        [Fact]
        public void FileNotFound_ContainsPlaceholder()
        {
            // Act
            string message = ValidationMessages.FileNotFound;

            // Assert
            Assert.Contains("{0}", message);
        }

        [Fact]
        public void ConflictingFileSpecificationMethods_ReturnsNonEmptyMessage()
        {
            // Act
            string message = ValidationMessages.ConflictingFileSpecificationMethods;

            // Assert
            Assert.NotNull(message);
            Assert.NotEmpty(message);
        }

        [Fact]
        public void InvalidEncodingName_ReturnsNonEmptyMessage()
        {
            // Act
            string message = ValidationMessages.InvalidEncodingName;

            // Assert
            Assert.NotNull(message);
            Assert.NotEmpty(message);
        }

        [Theory]
        [InlineData("ErrorsOccurred")]
        [InlineData("UnexpectedErrorOccurred")]
        [InlineData("FileNotFound")]
        [InlineData("UnknownEncoding")]
        public void ErrorMessages_ContainPlaceholder(string propertyName)
        {
            // Act
            var property = typeof(ValidationMessages).GetProperty(propertyName);
            Assert.NotNull(property);
            
            string message = property.GetValue(null) as string;

            // Assert
            Assert.NotNull(message);
            Assert.Contains("{0}", message);
        }
    }
}
