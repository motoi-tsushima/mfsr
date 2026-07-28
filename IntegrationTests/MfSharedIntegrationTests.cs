using System.Globalization;
using System.Text;
using System.Threading;
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

    /// <summary>
    /// 韓国語テキスト（CP949）のエンコーディング検出テスト
    /// </summary>
    [Fact]
    public void EncodingDetection_KoreanCP949_DetectsCorrectly()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "korean_cp949.txt");
        var koreanText = "안녕하세요! 한국어로 작성된 테스트 파일입니다. 이 파일은 CP949 인코딩을 사용합니다.";
        var cp949 = Encoding.GetEncoding(949); // CP949
        File.WriteAllText(testFile, koreanText, cp949);

        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try
        {
            // カルチャーを韓国語に設定（アプリで --culture ko-KR を指定した場合と同等）
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ko-KR");

            // Act
            using var fs = new FileStream(testFile, FileMode.Open, FileAccess.Read);
            var result = EncodingHelper.DetectOrUseSpecifiedEncoding(fs, testFile, null, MfCommon.EncodingDetectionType.Normal);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Encoding);
            // CP949 (949) または EUC-KR (51949) として検出されることを確認（両者は基本ハングル範囲で互換）
            Assert.True(result.CodePage == 949 || result.CodePage == 51949,
                $"Korean encoding expected (949 or 51949), but was {result.CodePage}");
            // 検出されたエンコーディングでテキストを正しくデコードできることを確認
            var bytes = File.ReadAllBytes(testFile);
            var decoded = result.Encoding.GetString(bytes);
            Assert.Contains("한국어", decoded);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    /// <summary>
    /// 韓国語テキスト（EUC-KR）のエンコーディング検出テスト
    /// </summary>
    [Fact]
    public void EncodingDetection_KoreanEucKR_DetectsCorrectly()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "korean_euckr.txt");
        var koreanText = "대한민국의 한국어 텍스트입니다. 이 파일은 EUC-KR 인코딩을 사용합니다.";
        var eucKr = Encoding.GetEncoding(51949); // EUC-KR
        File.WriteAllText(testFile, koreanText, eucKr);

        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ko-KR");

            // Act
            using var fs = new FileStream(testFile, FileMode.Open, FileAccess.Read);
            var result = EncodingHelper.DetectOrUseSpecifiedEncoding(fs, testFile, null, MfCommon.EncodingDetectionType.Normal);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Encoding);
            // CP949 (949) または EUC-KR (51949) として検出されることを確認（基本ハングル範囲では同一バイト列）
            Assert.True(result.CodePage == 949 || result.CodePage == 51949,
                $"Korean encoding expected (949 or 51949), but was {result.CodePage}");
            var bytes = File.ReadAllBytes(testFile);
            var decoded = result.Encoding.GetString(bytes);
            Assert.Contains("한국어", decoded);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    /// <summary>
    /// 中国語簡体字テキスト（GBK）のエンコーディング検出テスト
    /// </summary>
    [Fact]
    public void EncodingDetection_ChineseSimplifiedGBK_DetectsCorrectly()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "chinese_gbk.txt");
        var chineseText = "这是一个用简体中文编写的测试文件。这个文件使用GBK编码进行存储。";
        var gbk = Encoding.GetEncoding(936); // GBK
        File.WriteAllText(testFile, chineseText, gbk);

        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try
        {
            // カルチャーを中国語（簡体字）に設定
            Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-CN");

            // Act
            using var fs = new FileStream(testFile, FileMode.Open, FileAccess.Read);
            var result = EncodingHelper.DetectOrUseSpecifiedEncoding(fs, testFile, null, MfCommon.EncodingDetectionType.Normal);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Encoding);
            // GBK (936) または GB18030 (54936) として検出されることを確認（GB18030はGBKの上位互換）
            Assert.True(result.CodePage == 936 || result.CodePage == 54936,
                $"Simplified Chinese encoding expected (936 or 54936), but was {result.CodePage}");
            var bytes = File.ReadAllBytes(testFile);
            var decoded = result.Encoding.GetString(bytes);
            Assert.Contains("中文", decoded);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    /// <summary>
    /// 中国語簡体字テキスト（GB18030）のエンコーディング検出テスト
    /// </summary>
    [Fact]
    public void EncodingDetection_ChineseSimplifiedGB18030_DetectsCorrectly()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "chinese_gb18030.txt");
        var chineseText = "中华人民共和国。这是一个使用GB18030编码的简体中文测试文件。";
        var gb18030 = Encoding.GetEncoding(54936); // GB18030
        File.WriteAllText(testFile, chineseText, gb18030);

        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-CN");

            // Act
            using var fs = new FileStream(testFile, FileMode.Open, FileAccess.Read);
            var result = EncodingHelper.DetectOrUseSpecifiedEncoding(fs, testFile, null, MfCommon.EncodingDetectionType.Normal);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Encoding);
            // GBK (936) または GB18030 (54936) として検出されることを確認
            Assert.True(result.CodePage == 936 || result.CodePage == 54936,
                $"Simplified Chinese encoding expected (936 or 54936), but was {result.CodePage}");
            var bytes = File.ReadAllBytes(testFile);
            var decoded = result.Encoding.GetString(bytes);
            Assert.Contains("中文", decoded);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    /// <summary>
    /// 中国語繁体字テキスト（Big5）のエンコーディング検出テスト（台湾・香港）
    /// </summary>
    [Fact]
    public void EncodingDetection_ChineseTraditionalBig5_DetectsCorrectly()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "chinese_big5.txt");
        var chineseText = "這是一個用繁體中文編寫的測試檔案。此檔案使用Big5編碼進行儲存。";
        var big5 = Encoding.GetEncoding(950); // Big5
        File.WriteAllText(testFile, chineseText, big5);

        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try
        {
            // カルチャーを中国語（繁体字・台湾）に設定
            Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-TW");

            // Act
            using var fs = new FileStream(testFile, FileMode.Open, FileAccess.Read);
            var result = EncodingHelper.DetectOrUseSpecifiedEncoding(fs, testFile, null, MfCommon.EncodingDetectionType.Normal);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Encoding);
            Assert.Equal(950, result.CodePage); // Big5 (950)
            var bytes = File.ReadAllBytes(testFile);
            var decoded = result.Encoding.GetString(bytes);
            Assert.Contains("繁體中文", decoded);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    /// <summary>
    /// EUC-TW（台湾繁体字）のエンコーディング検出テスト
    /// EUC-TW (CodePage 51950) は .NET でサポートされていないため、
    /// EncodingProbe が 51950 を返した場合は Encoding == null になる
    /// </summary>
    [Fact]
    public void EncodingDetection_EucTW_CodePageDetected_EncodingIsNull()
    {
        // Arrange
        // EUC-TW のバイト列: CNS 11643 Plane 1 の文字を EUC-TW 形式でエンコード
        // EUC-TW の2バイト文字は両バイトとも 0xA1-0xFE の範囲
        // 以下は「中文測試」に相当する EUC-TW バイト列
        var eucTwBytes = new byte[]
        {
            // 中: CNS11643-1 → EUC-TW: 0xA4 0xE2
            0xA4, 0xE2,
            // 文: CNS11643-1 → EUC-TW: 0xA4, 0xE5
            0xA4, 0xE5,
            // 測: CNS11643-1 → EUC-TW: 0xB4, 0xFA
            0xB4, 0xFA,
            // 試: CNS11643-1 → EUC-TW: 0xB8, 0xD5
            0xB8, 0xD5,
            // 台: CNS11643-1 → EUC-TW: 0xA5, 0xD8
            0xA5, 0xD8,
            // 灣: CNS11643-1 → EUC-TW: 0xC6, 0xE3
            0xC6, 0xE3,
            // 繁: CNS11643-1 → EUC-TW: 0xC1, 0xFA
            0xC1, 0xFA,
            // 體: CNS11643-1 → EUC-TW: 0xC5, 0xD7
            0xC5, 0xD7,
        };
        var testFile = Path.Combine(_testDirectory, "euctw_test.txt");
        File.WriteAllBytes(testFile, eucTwBytes);

        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-TW");

            // Act
            using var fs = new FileStream(testFile, FileMode.Open, FileAccess.Read);
            var result = EncodingHelper.DetectOrUseSpecifiedEncoding(fs, testFile, null, MfCommon.EncodingDetectionType.Normal);

            // Assert
            Assert.NotNull(result);
            // EncodingProbe が EUC-TW (51950) と判定した場合、.NET 非サポートのため Encoding == null
            // Big5 (950) と判定した場合は Encoding != null
            if (result.CodePage == 51950)
            {
                // EUC-TW は .NET でサポートされていないため Encoding が null になることを確認
                Assert.Null(result.Encoding);
                Assert.NotNull(result.EncodingInfo);
            }
            else
            {
                // Big5 などの類似エンコーディングとして検出された場合
                Assert.True(result.CodePage == 950 || result.CodePage == 65001,
                    $"Expected EUC-TW (51950) or compatible encoding (950), but was {result.CodePage}");
            }
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
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
