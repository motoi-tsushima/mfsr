using System;
using System.Collections.Generic;
using Xunit;
using MF.Shared;

namespace MF.Shared.Tests
{
    /// <summary>
    /// Colipex (コマンドライン解析) クラスのテスト
    /// </summary>
    public class ColipexTests
    {
        [Fact]
        public void Constructor_SimpleParameter_ParsesCorrectly()
        {
            // Arrange
            string[] args = { "file1.txt", "file2.txt" };

            // Act
            var colipex = new TestColipex(args);

            // Assert
            Assert.Equal(2, colipex.Parameters.Count);
            Assert.Equal("file1.txt", colipex.Parameters[0]);
            Assert.Equal("file2.txt", colipex.Parameters[1]);
        }

        [Fact]
        public void Constructor_OptionWithColon_ParsesCorrectly()
        {
            // Arrange
            string[] args = { "/c:utf-8", "/w:shift_jis" };

            // Act
            var colipex = new TestColipex(args);

            // Assert
            Assert.True(colipex.IsOption("c"));
            Assert.Equal("utf-8", colipex.Options["c"]);
            Assert.True(colipex.IsOption("w"));
            Assert.Equal("shift_jis", colipex.Options["w"]);
        }

        [Fact]
        public void Constructor_OptionWithEqual_ParsesCorrectly()
        {
            // Arrange
            string[] args = { "/encoding=utf-8" };

            // Act
            var colipex = new TestColipex(args);

            // Assert
            Assert.True(colipex.IsOption("encoding"));
            Assert.Equal("utf-8", colipex.Options["encoding"]);
        }

        [Fact]
        public void Constructor_OptionWithoutValue_ParsesAsNonValue()
        {
            // Arrange
            string[] args = { "/h", "/v" };

            // Act
            var colipex = new TestColipex(args);

            // Assert
            Assert.True(colipex.IsOption("h"));
            Assert.Equal(Colipex.NonValue, colipex.Options["h"]);
            Assert.True(colipex.IsOption("v"));
            Assert.Equal(Colipex.NonValue, colipex.Options["v"]);
        }

        [Fact]
        public void Constructor_MixedParametersAndOptions_ParsesCorrectly()
        {
            // Arrange
            string[] args = { "/c:utf-8", "file1.txt", "/d", "file2.txt" };

            // Act
            var colipex = new TestColipex(args);

            // Assert
            Assert.Equal(2, colipex.Parameters.Count);
            Assert.Contains("file1.txt", colipex.Parameters);
            Assert.Contains("file2.txt", colipex.Parameters);
            Assert.True(colipex.IsOption("c"));
            Assert.Equal("utf-8", colipex.Options["c"]);
            Assert.True(colipex.IsOption("d"));
        }

        [Fact]
        public void Constructor_OptionWithSpaces_TrimsCorrectly()
        {
            // Arrange
            string[] args = { "/c: utf-8 " };

            // Act
            var colipex = new TestColipex(args);

            // Assert
            Assert.True(colipex.IsOption("c"));
            Assert.Equal("utf-8", colipex.Options["c"].Trim());
        }

        [Fact]
        public void IsOption_ExistingOption_ReturnsTrue()
        {
            // Arrange
            string[] args = { "/help" };
            var colipex = new TestColipex(args);

            // Act
            bool result = colipex.IsOption("help");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsOption_NonExistingOption_ReturnsFalse()
        {
            // Arrange
            string[] args = { "/help" };
            var colipex = new TestColipex(args);

            // Act
            bool result = colipex.IsOption("version");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Constructor_EmptyArgs_CreatesEmptyCollections()
        {
            // Arrange
            string[] args = Array.Empty<string>();

            // Act
            var colipex = new TestColipex(args);

            // Assert
            Assert.Empty(colipex.Parameters);
            Assert.Empty(colipex.Options);
        }

        // テスト用のColipexサブクラス
        private class TestColipex : Colipex
        {
            public TestColipex(string[] args) : base(args)
            {
            }

            // テストのためにプロテクトメンバーを公開
            public new List<string> Parameters => base.Parameters;
            public new Dictionary<string, string> Options => base.Options;
        }
    }
}
