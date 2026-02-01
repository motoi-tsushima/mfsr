using System.Text;
using Xunit;
using mfsr;

namespace IntegrationTests;

/// <summary>
/// mfsr の統合テスト
/// </summary>
public class MfsrIntegrationTests : IDisposable
{
    private readonly string _testDirectory;

    static MfsrIntegrationTests()
    {
        // Shift_JIS などのレガシーエンコーディングを使用可能にする
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public MfsrIntegrationTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"mfsr_test_{Guid.NewGuid()}");
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
    public void ReplaceStringsInFiles_SimpleReplace_ReplacesText()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "replace_test.txt");
        var originalContent = "Hello World! Hello Universe!";
        File.WriteAllText(testFile, originalContent, Encoding.UTF8);

        var replaceWords = new string[2, 1] { { "Hello" }, { "Hi" } };
        var files = new[] { testFile };

        var replacer = new ReplaceStringsInFiles(replaceWords, files, null);

        // Act
        var result = replacer.Replace(Encoding.UTF8, Encoding.UTF8, "\r\n");

        // Assert
        Assert.True(result);
        var content = File.ReadAllText(testFile);
        Assert.Equal("Hi World! Hi Universe!", content);
    }

    [Fact]
    public void ReplaceStringsInFiles_MultipleReplacements_ReplacesAllOccurrences()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "multi_replace.txt");
        var originalContent = "apple banana apple orange";
        File.WriteAllText(testFile, originalContent, Encoding.UTF8);

        var replaceWords = new string[2, 2] 
        { 
            { "apple", "banana" },
            { "grape", "melon" }
        };
        var files = new[] { testFile };

        var replacer = new ReplaceStringsInFiles(replaceWords, files, null);

        // Act
        var result = replacer.Replace(Encoding.UTF8, Encoding.UTF8, "\r\n");

        // Assert
        Assert.True(result);
        var content = File.ReadAllText(testFile);
        Assert.Equal("grape melon grape orange", content);
    }

    [Fact]
    public void ReplaceStringsInFiles_JapaneseText_ReplacesCorrectly()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "japanese_test.txt");
        var originalContent = "これはテストです。テストを実行します。";
        File.WriteAllText(testFile, originalContent, Encoding.UTF8);

        var replaceWords = new string[2, 1] { { "テスト" }, { "試験" } };
        var files = new[] { testFile };

        var replacer = new ReplaceStringsInFiles(replaceWords, files, null);

        // Act
        var result = replacer.Replace(Encoding.UTF8, Encoding.UTF8, "\r\n");

        // Assert
        Assert.True(result);
        var content = File.ReadAllText(testFile);
        Assert.Equal("これは試験です。試験を実行します。", content);
    }

    [Fact]
    public void ReplaceStringsInFiles_ShiftJISToUTF8_ConvertsEncoding()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "sjis_to_utf8.txt");
        var sjisEncoding = Encoding.GetEncoding("Shift_JIS");
        var originalContent = "Shift_JISのファイルです";
        File.WriteAllText(testFile, originalContent, sjisEncoding);

        var replaceWords = new string[2, 1] { { "ファイル" }, { "テキスト" } };
        var files = new[] { testFile };

        var replacer = new ReplaceStringsInFiles(replaceWords, files, null);

        // Act
        var result = replacer.Replace(sjisEncoding, Encoding.UTF8, "\r\n");

        // Assert
        Assert.True(result);
        var content = File.ReadAllText(testFile, Encoding.UTF8);
        Assert.Contains("テキスト", content);
    }

    [Fact]
    public void ReplaceStringsInFiles_AddBOM_AddsUTF8BOM()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "add_bom.txt");
        File.WriteAllText(testFile, "Test content", new UTF8Encoding(false));

        var replaceWords = new string[2, 1] { { "Test" }, { "Modified" } };
        var files = new[] { testFile };

        var replacer = new ReplaceStringsInFiles(replaceWords, files, true);

        // Act
        var result = replacer.Replace(Encoding.UTF8, new UTF8Encoding(true), "\r\n");

        // Assert
        Assert.True(result);
        var bytes = File.ReadAllBytes(testFile);
        Assert.Equal(0xEF, bytes[0]);
        Assert.Equal(0xBB, bytes[1]);
        Assert.Equal(0xBF, bytes[2]);
    }

    [Fact]
    public void ReplaceStringsInFiles_RemoveBOM_RemovesUTF8BOM()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "remove_bom.txt");
        File.WriteAllText(testFile, "Test content", new UTF8Encoding(true));

        var replaceWords = new string[2, 1] { { "Test" }, { "Modified" } };
        var files = new[] { testFile };

        var replacer = new ReplaceStringsInFiles(replaceWords, files, false);

        // Act
        var result = replacer.Replace(Encoding.UTF8, new UTF8Encoding(false), "\r\n");

        // Assert
        Assert.True(result);
        var bytes = File.ReadAllBytes(testFile);
        Assert.NotEqual(0xEF, bytes[0]);
    }

    [Fact]
    public void ReplaceStringsInFiles_ChangeLineEndings_ConvertsToLF()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "line_endings.txt");
        var originalContent = "Line1\r\nLine2\r\nLine3";
        File.WriteAllText(testFile, originalContent, Encoding.UTF8);

        var replaceWords = new string[2, 1] { { "Line" }, { "Row" } };
        var files = new[] { testFile };

        var replacer = new ReplaceStringsInFiles(replaceWords, files, null);

        // Act
        var result = replacer.Replace(Encoding.UTF8, Encoding.UTF8, "\n");

        // Assert
        Assert.True(result);
        var content = File.ReadAllText(testFile);
        Assert.Contains("Row1\nRow2\nRow3", content);
        Assert.DoesNotContain("\r\n", content);
    }

    [Fact]
    public void ReplaceStringsInFiles_MultipleFiles_ReplacesAllFiles()
    {
        // Arrange
        var files = new List<string>();
        for (int i = 0; i < 5; i++)
        {
            var file = Path.Combine(_testDirectory, $"file{i}.txt");
            File.WriteAllText(file, $"Content {i} with old text", Encoding.UTF8);
            files.Add(file);
        }

        var replaceWords = new string[2, 1] { { "old" }, { "new" } };
        var replacer = new ReplaceStringsInFiles(replaceWords, files.ToArray(), null);

        // Act
        var result = replacer.Replace(Encoding.UTF8, Encoding.UTF8, "\r\n");

        // Assert
        Assert.True(result);
        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            Assert.Contains("new text", content);
            Assert.DoesNotContain("old text", content);
        }
    }

    [Fact]
    public void ReplaceStringsInFiles_EmptyReplacement_RemovesText()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "empty_replace.txt");
        File.WriteAllText(testFile, "Remove this word from text", Encoding.UTF8);

        var replaceWords = new string[2, 1] { { "word " }, { "" } };
        var files = new[] { testFile };

        var replacer = new ReplaceStringsInFiles(replaceWords, files, null);

        // Act
        var result = replacer.Replace(Encoding.UTF8, Encoding.UTF8, "\r\n");

        // Assert
        Assert.True(result);
        var content = File.ReadAllText(testFile);
        Assert.Equal("Remove this from text", content);
    }

    [Fact]
    public void ReplaceStringsInFiles_SpecialCharacters_ReplacesCorrectly()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "special_chars.txt");
        File.WriteAllText(testFile, "Price: $100.00 (discount: 10%)", Encoding.UTF8);

        var replaceWords = new string[2, 2] 
        { 
            { "$100.00", "10%" },
            { "$80.00", "20%" }
        };
        var files = new[] { testFile };

        var replacer = new ReplaceStringsInFiles(replaceWords, files, null);

        // Act
        var result = replacer.Replace(Encoding.UTF8, Encoding.UTF8, "\r\n");

        // Assert
        Assert.True(result);
        var content = File.ReadAllText(testFile);
        Assert.Equal("Price: $80.00 (discount: 20%)", content);
    }
}
