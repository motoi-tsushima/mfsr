using System.Text;
using Xunit;
using mfprobe;
using mfsr;

namespace IntegrationTests;

/// <summary>
/// mfprobe と mfsr を組み合わせたエンドツーエンド統合テスト
/// </summary>
public class EndToEndIntegrationTests : IDisposable
{
    private readonly string _testDirectory;

    static EndToEndIntegrationTests()
    {
        // Shift_JIS などのレガシーエンコーディングを使用可能にする
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public EndToEndIntegrationTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"e2e_test_{Guid.NewGuid()}");
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
    public void ProbeAndReplace_FindsAndReplacesText()
    {
        // Arrange - 複数のファイルを作成
        var file1 = Path.Combine(_testDirectory, "doc1.txt");
        var file2 = Path.Combine(_testDirectory, "doc2.txt");
        var file3 = Path.Combine(_testDirectory, "doc3.txt");
        var probeOutput = Path.Combine(_testDirectory, "probe_results.txt");

        File.WriteAllText(file1, "This document contains TODO items", Encoding.UTF8);
        File.WriteAllText(file2, "No special markers here", Encoding.UTF8);
        File.WriteAllText(file3, "Another TODO that needs attention", Encoding.UTF8);

        var allFiles = new[] { file1, file2, file3 };

        // Act - Step 1: mfprobe で TODO を含むファイルを検索
        var searchWords = new[] { "TODO" };
        var probe = new ProbeFiles(searchWords, allFiles, true, probeOutput, Encoding.UTF8);
        var probeResult = probe.Probe(Encoding.UTF8);

        Assert.True(probeResult);
        Assert.True(File.Exists(probeOutput));

        // Step 2: 検索結果を読み取る
        var probeResultContent = File.ReadAllText(probeOutput);
        Assert.Contains(file1, probeResultContent);
        Assert.Contains(file3, probeResultContent);

        // Step 3: mfsr で TODO を DONE に置換
        var replaceWords = new string[2, 1] { { "TODO" }, { "DONE" } };
        var filesToReplace = new[] { file1, file3 };
        var replacer = new ReplaceStringsInFiles(replaceWords, filesToReplace, null);
        var replaceResult = replacer.Replace(Encoding.UTF8, Encoding.UTF8, "\r\n");

        // Assert
        Assert.True(replaceResult);
        Assert.Contains("DONE", File.ReadAllText(file1));
        Assert.Contains("DONE", File.ReadAllText(file3));
        Assert.DoesNotContain("TODO", File.ReadAllText(file1));
        Assert.DoesNotContain("TODO", File.ReadAllText(file3));
        Assert.DoesNotContain("TODO", File.ReadAllText(file2));
    }

    [Fact]
    public void MultilingualProbeAndReplace_HandlesJapaneseAndEnglish()
    {
        // Arrange
        var jpFile = Path.Combine(_testDirectory, "japanese.txt");
        var enFile = Path.Combine(_testDirectory, "english.txt");
        var probeOutput = Path.Combine(_testDirectory, "multilingual_probe.txt");

        File.WriteAllText(jpFile, "これは重要なドキュメントです。重要事項を確認してください。", Encoding.UTF8);
        File.WriteAllText(enFile, "This is an important document. Please review important items.", Encoding.UTF8);

        var allFiles = new[] { jpFile, enFile };

        // Act - 日本語と英語で「重要」を検索
        var searchWords = new[] { "重要", "important" };
        var probe = new ProbeFiles(searchWords, allFiles, true, probeOutput, Encoding.UTF8);
        probe.Probe(Encoding.UTF8);

        // 両方のファイルで置換を実行
        var replaceWords = new string[2, 2] 
        { 
            { "重要", "important" },
            { "必須", "critical" }
        };
        var replacer = new ReplaceStringsInFiles(replaceWords, allFiles, null);
        replacer.Replace(Encoding.UTF8, Encoding.UTF8, "\r\n");

        // Assert
        var jpContent = File.ReadAllText(jpFile);
        var enContent = File.ReadAllText(enFile);

        Assert.Contains("必須", jpContent);
        Assert.DoesNotContain("重要", jpContent);
        Assert.Contains("critical", enContent);
        Assert.DoesNotContain("important", enContent);
    }

    [Fact]
    public void EncodingConversion_ProbeInShiftJISAndConvertToUTF8()
    {
        // Arrange
        var sjisFile = Path.Combine(_testDirectory, "shiftjis.txt");
        var probeOutput = Path.Combine(_testDirectory, "sjis_probe.txt");
        var sjisEncoding = Encoding.GetEncoding("Shift_JIS");

        File.WriteAllText(sjisFile, "Shift_JISでエンコードされたファイル。変換対象テキスト。", sjisEncoding);

        // Act - Shift_JIS でプローブ
        var searchWords = new[] { "変換対象" };
        var probe = new ProbeFiles(searchWords, new[] { sjisFile }, true, probeOutput, Encoding.UTF8);
        probe.Probe(sjisEncoding);

        // Shift_JIS から UTF-8 に変換しながら置換
        var replaceWords = new string[2, 1] { { "変換対象" }, { "変換済み" } };
        var replacer = new ReplaceStringsInFiles(replaceWords, new[] { sjisFile }, null);
        replacer.Replace(sjisEncoding, Encoding.UTF8, "\r\n");

        // Assert
        var content = File.ReadAllText(sjisFile, Encoding.UTF8);
        Assert.Contains("変換済み", content);

        // ファイルが UTF-8 になっていることを確認
        var bytes = File.ReadAllBytes(sjisFile);
        var detectedContent = Encoding.UTF8.GetString(bytes);
        Assert.Contains("変換済み", detectedContent);
    }

    [Fact]
    public void BulkProcessing_HandlesLargeNumberOfFiles()
    {
        // Arrange - 100個のファイルを作成
        var files = new List<string>();
        var probeOutput = Path.Combine(_testDirectory, "bulk_probe.txt");

        for (int i = 0; i < 100; i++)
        {
            var file = Path.Combine(_testDirectory, $"bulk_{i}.txt");
            var content = i % 3 == 0 
                ? $"File {i} with marker keyword" 
                : $"File {i} without special word";
            File.WriteAllText(file, content, Encoding.UTF8);
            files.Add(file);
        }

        // Act - すべてのファイルをプローブ
        var searchWords = new[] { "marker" };
        var probe = new ProbeFiles(searchWords, files.ToArray(), true, probeOutput, Encoding.UTF8);
        probe.Probe(Encoding.UTF8);

        // marker を含むファイルのみ置換
        var replaceWords = new string[2, 1] { { "marker" }, { "updated" } };
        var replacer = new ReplaceStringsInFiles(replaceWords, files.ToArray(), null);
        replacer.Replace(Encoding.UTF8, Encoding.UTF8, "\r\n");

        // Assert
        var probeResultContent = File.ReadAllText(probeOutput);
        int updatedCount = 0;

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            if (content.Contains("updated"))
            {
                updatedCount++;
                Assert.DoesNotContain("marker", content);
            }
        }

        Assert.True(updatedCount > 0);
        Assert.Equal(34, updatedCount); // 0, 3, 6, 9, ... 99 = 34 files
    }

    [Fact]
    public void LineEndingConversion_ProbeAndConvertCRLFToLF()
    {
        // Arrange
        var file1 = Path.Combine(_testDirectory, "crlf1.txt");
        var file2 = Path.Combine(_testDirectory, "crlf2.txt");
        var probeOutput = Path.Combine(_testDirectory, "crlf_probe.txt");

        File.WriteAllText(file1, "Line1\r\nLine2\r\nLine3 with target", Encoding.UTF8);
        File.WriteAllText(file2, "Line1\r\nLine2\r\nLine3", Encoding.UTF8);

        var files = new[] { file1, file2 };

        // Act - target を含むファイルを検索
        var searchWords = new[] { "target" };
        var probe = new ProbeFiles(searchWords, files, true, probeOutput, Encoding.UTF8);
        probe.Probe(Encoding.UTF8);

        // すべてのファイルで CRLF を LF に変換
        var replaceWords = new string[2, 1] { { "target" }, { "keyword" } };
        var replacer = new ReplaceStringsInFiles(replaceWords, files, null);
        replacer.Replace(Encoding.UTF8, Encoding.UTF8, "\n");

        // Assert
        var content1 = File.ReadAllText(file1);
        var content2 = File.ReadAllText(file2);

        Assert.DoesNotContain("\r\n", content1);
        Assert.DoesNotContain("\r\n", content2);
        Assert.Contains("\n", content1);
        Assert.Contains("\n", content2);
        Assert.Contains("keyword", content1);
    }

    [Fact]
    public void BOMHandling_ProbeAndAddBOMToMatchingFiles()
    {
        // Arrange
        var file1 = Path.Combine(_testDirectory, "nobom1.txt");
        var file2 = Path.Combine(_testDirectory, "nobom2.txt");
        var probeOutput = Path.Combine(_testDirectory, "bom_probe.txt");

        File.WriteAllText(file1, "UTF-8 without BOM containing special", new UTF8Encoding(false));
        File.WriteAllText(file2, "UTF-8 without BOM normal", new UTF8Encoding(false));

        var files = new[] { file1, file2 };

        // Act - special を含むファイルを検索
        var searchWords = new[] { "special" };
        var probe = new ProbeFiles(searchWords, files, true, probeOutput, Encoding.UTF8);
        probe.Probe(Encoding.UTF8);

        // 検索でヒットしたファイルに BOM を追加
        var replaceWords = new string[2, 1] { { "special" }, { "marked" } };
        var replacer = new ReplaceStringsInFiles(replaceWords, new[] { file1 }, true);
        replacer.Replace(Encoding.UTF8, new UTF8Encoding(true), "\r\n");

        // Assert
        var bytes1 = File.ReadAllBytes(file1);
        var bytes2 = File.ReadAllBytes(file2);

        // file1 は BOM あり
        Assert.Equal(0xEF, bytes1[0]);
        Assert.Equal(0xBB, bytes1[1]);
        Assert.Equal(0xBF, bytes1[2]);

        // file2 は BOM なし
        Assert.NotEqual(0xEF, bytes2[0]);
    }

    [Fact]
    public void ComplexScenario_MultiStepProcessing()
    {
        // Arrange - 複雑なシナリオ: 検索 → 置換 → 再検索
        var file1 = Path.Combine(_testDirectory, "complex1.txt");
        var file2 = Path.Combine(_testDirectory, "complex2.txt");
        var probeOutput1 = Path.Combine(_testDirectory, "probe1.txt");
        var probeOutput2 = Path.Combine(_testDirectory, "probe2.txt");

        File.WriteAllText(file1, "Phase1: draft version TODO review", Encoding.UTF8);
        File.WriteAllText(file2, "Phase1: final version", Encoding.UTF8);

        var files = new[] { file1, file2 };

        // Step 1: draft を含むファイルを検索
        var probe1 = new ProbeFiles(new[] { "draft" }, files, true, probeOutput1, Encoding.UTF8);
        probe1.Probe(Encoding.UTF8);

        // Step 2: draft → final に置換
        var replacer1 = new ReplaceStringsInFiles(new string[2, 1] { { "draft" }, { "final" } }, files, null);
        replacer1.Replace(Encoding.UTF8, Encoding.UTF8, "\r\n");

        // Step 3: TODO を含むファイルを再検索
        var probe2 = new ProbeFiles(new[] { "TODO" }, files, true, probeOutput2, Encoding.UTF8);
        probe2.Probe(Encoding.UTF8);

        // Step 4: TODO → DONE に置換
        var replacer2 = new ReplaceStringsInFiles(new string[2, 1] { { "TODO" }, { "DONE" } }, new[] { file1 }, null);
        replacer2.Replace(Encoding.UTF8, Encoding.UTF8, "\r\n");

        // Assert
        var content1 = File.ReadAllText(file1);
        var content2 = File.ReadAllText(file2);

        Assert.Equal("Phase1: final version DONE review", content1);
        Assert.Equal("Phase1: final version", content2);
    }
}
