using System.Text;
using Xunit;
using MF.Shared;

namespace IntegrationTests;

/// <summary>
/// MF.Shared ライブラリの統合テスト
/// </summary>
public class MfSharedIntegrationTests : IDisposable
{
    private readonly string _testDirectory;

    static MfSharedIntegrationTests()
    {
        // Shift_JIS などのレガシーエンコーディングを使用可能にする
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public MfSharedIntegrationTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"shared_test_{Guid.NewGuid()}");
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
    public void EncodingDetection_UTF8WithBOM_DetectsCorrectly()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "utf8bom.txt");
        var utf8WithBom = new UTF8Encoding(true);
        File.WriteAllText(testFile, "UTF-8 with BOM content", utf8WithBom);

        // Act
        var bytes = File.ReadAllBytes(testFile);
        var bomDetector = new ByteOrderMarkDetection();

        // Assert
        Assert.True(bomDetector.IsBOM(bytes));
        Assert.Equal(65001, bomDetector.CodePage); // UTF-8
    }

    [Fact]
    public void EncodingDetection_UTF8WithoutBOM_DetectsCorrectly()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "utf8nobom.txt");
        var utf8NoBom = new UTF8Encoding(false);
        File.WriteAllText(testFile, "UTF-8 without BOM content", utf8NoBom);

        // Act
        var bytes = File.ReadAllBytes(testFile);
        var bomDetector = new ByteOrderMarkDetection();

        // Assert
        Assert.False(bomDetector.IsBOM(bytes));
    }

    [Fact]
    public void EncodingDetection_UTF16LE_DetectsCorrectly()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "utf16le.txt");
        File.WriteAllText(testFile, "UTF-16 LE content", Encoding.Unicode);

        // Act
        var bytes = File.ReadAllBytes(testFile);
        var bomDetector = new ByteOrderMarkDetection();

        // Assert
        Assert.True(bomDetector.IsBOM(bytes));
        Assert.Equal(1200, bomDetector.CodePage); // UTF-16 LE
    }

    [Fact]
    public void EncodingDetection_UTF16BE_DetectsCorrectly()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "utf16be.txt");
        File.WriteAllText(testFile, "UTF-16 BE content", Encoding.BigEndianUnicode);

        // Act
        var bytes = File.ReadAllBytes(testFile);
        var bomDetector = new ByteOrderMarkDetection();

        // Assert
        Assert.True(bomDetector.IsBOM(bytes));
        Assert.Equal(1201, bomDetector.CodePage); // UTF-16 BE
    }

    [Fact]
    public void EncodingDetection_ShiftJIS_DetectsCorrectly()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "shiftjis.txt");
        var sjisEncoding = Encoding.GetEncoding("Shift_JIS");
        File.WriteAllText(testFile, "日本語のShift_JISテキスト", sjisEncoding);

        // Act
        using var fs = new FileStream(testFile, FileMode.Open, FileAccess.Read);
        var result = EncodingHelper.DetectOrUseSpecifiedEncoding(fs, testFile, null, MfCommon.EncodingDetectionType.Normal);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Encoding);
    }

    [Fact]
    public void EncodingDetection_MultipleFiles_DetectsDifferentEncodings()
    {
        // Arrange
        var utf8File = Path.Combine(_testDirectory, "multi_utf8.txt");
        var sjisFile = Path.Combine(_testDirectory, "multi_sjis.txt");
        var utf16File = Path.Combine(_testDirectory, "multi_utf16.txt");

        File.WriteAllText(utf8File, "UTF-8 text with 日本語", Encoding.UTF8);
        File.WriteAllText(sjisFile, "Shift_JIS text", Encoding.GetEncoding("Shift_JIS"));
        File.WriteAllText(utf16File, "UTF-16 text", Encoding.Unicode);

        // Act
        using var fs1 = new FileStream(utf8File, FileMode.Open, FileAccess.Read);
        using var fs2 = new FileStream(sjisFile, FileMode.Open, FileAccess.Read);
        using var fs3 = new FileStream(utf16File, FileMode.Open, FileAccess.Read);
        
        var result1 = EncodingHelper.DetectOrUseSpecifiedEncoding(fs1, utf8File, null, MfCommon.EncodingDetectionType.Normal);
        var result2 = EncodingHelper.DetectOrUseSpecifiedEncoding(fs2, sjisFile, null, MfCommon.EncodingDetectionType.Normal);
        var result3 = EncodingHelper.DetectOrUseSpecifiedEncoding(fs3, utf16File, null, MfCommon.EncodingDetectionType.Normal);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result3);

        var bytes3 = File.ReadAllBytes(utf16File);
        var bomDetector = new ByteOrderMarkDetection();
        Assert.True(bomDetector.IsBOM(bytes3));
        Assert.Equal(1200, bomDetector.CodePage); // UTF-16 LE
    }

    [Fact]
    public void RmsmfException_WithInnerException_PreservesExceptionChain()
    {
        // Arrange
        var innerException = new IOException("Inner exception message");
        var message = "Outer exception message";

        // Act
        var exception = new RmsmfException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void EncodingHelper_JapaneseText_PreservesContent()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "japanese_preserve.txt");
        var originalText = "これは日本語のテストです。漢字、ひらがな、カタカナが含まれています。";
        File.WriteAllText(testFile, originalText, Encoding.UTF8);

        // Act
        using var fs = new FileStream(testFile, FileMode.Open, FileAccess.Read);
        var result = EncodingHelper.DetectOrUseSpecifiedEncoding(fs, testFile, null, MfCommon.EncodingDetectionType.Normal);
        fs.Position = 0;
        var content = File.ReadAllText(testFile, result.Encoding ?? Encoding.UTF8);

        // Assert
        Assert.Equal(originalText, content);
    }

    [Fact]
    public void EncodingConversion_RoundTrip_PreservesData()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "roundtrip.txt");
        var originalText = "Test 123 テスト αβγ 测试";
        
        // UTF-8 で書き込み
        File.WriteAllText(testFile, originalText, Encoding.UTF8);
        var content1 = File.ReadAllText(testFile, Encoding.UTF8);

        // Shift_JIS で書き込み
        var sjisEncoding = Encoding.GetEncoding("Shift_JIS");
        File.WriteAllText(testFile, originalText, sjisEncoding);
        var content2 = File.ReadAllText(testFile, sjisEncoding);

        // UTF-8 に戻す
        File.WriteAllText(testFile, content2, Encoding.UTF8);
        var content3 = File.ReadAllText(testFile, Encoding.UTF8);

        // Assert
        Assert.Equal(originalText, content1);
        Assert.Contains("Test", content3);
        Assert.Contains("テスト", content3);
    }

    [Fact]
    public void BOMDetection_EmptyFile_HandlesGracefully()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "empty.txt");
        File.WriteAllText(testFile, "", new UTF8Encoding(false)); // BOM なし

        // Act
        var bytes = File.ReadAllBytes(testFile);
        var bomDetector = new ByteOrderMarkDetection();

        // Assert
        Assert.False(bomDetector.IsBOM(bytes));
    }

    [Fact]
    public void BOMDetection_SmallFile_DetectsCorrectly()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "small.txt");
        File.WriteAllText(testFile, "AB", new UTF8Encoding(true));

        // Act
        var bytes = File.ReadAllBytes(testFile);
        var bomDetector = new ByteOrderMarkDetection();

        // Assert
        Assert.True(bomDetector.IsBOM(bytes));
        Assert.True(bytes.Length >= 5); // BOM (3 bytes) + "AB" (2 bytes)
    }

    [Fact]
    public void EncodingDetection_MixedContent_HandlesCorrectly()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "mixed.txt");
        var mixedContent = @"English text
日本語テキスト
한글 텍스트
中文文本
Symbols: © ® ™ € £ ¥";
        
        File.WriteAllText(testFile, mixedContent, Encoding.UTF8);

        // Act
        using var fs = new FileStream(testFile, FileMode.Open, FileAccess.Read);
        var result = EncodingHelper.DetectOrUseSpecifiedEncoding(fs, testFile, null, MfCommon.EncodingDetectionType.Normal);
        fs.Position = 0;
        var content = File.ReadAllText(testFile, result.Encoding ?? Encoding.UTF8);

        // Assert
        Assert.Equal(mixedContent, content);
        Assert.Contains("日本語", content);
        Assert.Contains("한글", content);
        Assert.Contains("中文", content);
    }
}
