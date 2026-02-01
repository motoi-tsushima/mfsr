using System;
using Xunit;
using MF.Shared;

namespace MF.Shared.Tests
{
    /// <summary>
    /// RmsmfException クラスのテスト
    /// </summary>
    public class RmsmfExceptionTests
    {
        [Fact]
        public void Constructor_WithMessage_CreatesException()
        {
            // Arrange
            string expectedMessage = "Test error message";

            // Act
            var exception = new RmsmfException(expectedMessage);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void Constructor_WithMessageAndInnerException_CreatesException()
        {
            // Arrange
            string expectedMessage = "Test error message";
            var innerException = new InvalidOperationException("Inner error");

            // Act
            var exception = new RmsmfException(expectedMessage, innerException);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
            Assert.Same(innerException, exception.InnerException);
        }

        [Fact]
        public void Constructor_Default_CreatesException()
        {
            // Act
            var exception = new RmsmfException();

            // Assert
            Assert.NotNull(exception);
        }
    }
}
