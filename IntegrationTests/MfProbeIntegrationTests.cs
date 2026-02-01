using System.Text;
using Xunit;
using mfprobe;

namespace IntegrationTests;

/// <summary>
/// mfprobe の統合テスト
/// </summary>
public class MfProbeIntegrationTests : IDisposable
{
    private readonly string _testDirectory;

    static MfProbeIntegrationTests()
    {
        // Shift_JIS などのレガシーエンコーディングを使用可能にする
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public MfProbeIntegrationTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"mfprobe_test_{Guid.NewGuid()}");
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
    public void ProbeFiles_UTF8Files_FindsSearchWords()
    {
        // Arrange
        var testFile1 = Path.Combine(_testDirectory, "test1.txt");
        var testFile2 = Path.Combine(_testDirectory, "test2.txt");
        var outputFile = Path.Combine(_testDirectory, "output.txt");

        File.WriteAllText(testFile1, "This is a test file with keyword hello", Encoding.UTF8);
        File.WriteAllText(testFile2, "Another file without the target word", Encoding.UTF8);

        var searchWords = new[] { "hello" };
        var files = new[] { testFile1, testFile2 };

        var probe = new ProbeFiles(searchWords, files, true, outputFile, Encoding.UTF8);

        // Act
        var result = probe.Probe(Encoding.UTF8);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(outputFile));
        var output = File.ReadAllText(outputFile);
        Assert.Contains(testFile1, output);
    }

    [Fact]
    public void ProbeFiles_MultipleSearchWords_FindsAllMatches()
    {
        // Arrange
        var testFile1 = Path.Combine(_testDirectory, "multi1.txt");
        var testFile2 = Path.Combine(_testDirectory, "multi2.txt");
        var outputFile = Path.Combine(_testDirectory, "multi_output.txt");

        File.WriteAllText(testFile1, "This file contains apple and banana", Encoding.UTF8);
        File.WriteAllText(testFile2, "This file has orange", Encoding.UTF8);

        var searchWords = new[] { "apple", "orange" };
        var files = new[] { testFile1, testFile2 };

        var probe = new ProbeFiles(searchWords, files, true, outputFile, Encoding.UTF8);

        // Act
        var result = probe.Probe(Encoding.UTF8);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(outputFile));
        var output = File.ReadAllText(outputFile);
        Assert.Contains(testFile1, output);
        Assert.Contains(testFile2, output);
    }

    [Fact]
    public void ProbeFiles_ShiftJISFile_FindsJapaneseText()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "sjis_test.txt");
        var outputFile = Path.Combine(_testDirectory, "sjis_output.txt");

        var sjisEncoding = Encoding.GetEncoding("Shift_JIS");
        File.WriteAllText(testFile, "日本語のテストファイルです。検索", sjisEncoding);

        var searchWords = new[] { "検索" };
        var files = new[] { testFile };

        var probe = new ProbeFiles(searchWords, files, true, outputFile, Encoding.UTF8);

        // Act
        var result = probe.Probe(sjisEncoding);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(outputFile));
    }

    [Fact]
    public void ProbeFiles_MultipleFiles_HandlesParallelProcessing()
    {
        // Arrange
        var outputFile = Path.Combine(_testDirectory, "parallel_output.txt");
        var files = new List<string>();

        for (int i = 0; i < 10; i++)
        {
            var file = Path.Combine(_testDirectory, $"parallel_{i}.txt");
            File.WriteAllText(file, $"File {i} contains target keyword", Encoding.UTF8);
            files.Add(file);
        }

        var searchWords = new[] { "target" };
        var probe = new ProbeFiles(searchWords, files.ToArray(), true, outputFile, Encoding.UTF8);

        // Act
        var result = probe.Probe(Encoding.UTF8);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(outputFile));
        var output = File.ReadAllText(outputFile);
        foreach (var file in files)
        {
            Assert.Contains(Path.GetFileName(file), output);
        }
    }

    [Fact]
    public void ProbeFiles_UTF8WithBOM_DetectsEncoding()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "utf8bom.txt");
        var outputFile = Path.Combine(_testDirectory, "utf8bom_output.txt");

        var utf8WithBom = new UTF8Encoding(true);
        File.WriteAllText(testFile, "UTF-8 with BOM test file", utf8WithBom);

        var searchWords = new[] { "test" };
        var files = new[] { testFile };

        var probe = new ProbeFiles(searchWords, files, true, outputFile, Encoding.UTF8);

        // Act
        var result = probe.Probe(Encoding.UTF8);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(outputFile));
    }

    [Fact]
    public void ProbeFiles_NoMatches_CreatesEmptyOutput()
    {
        // Arrange
        var testFile = Path.Combine(_testDirectory, "nomatch.txt");
        var outputFile = Path.Combine(_testDirectory, "nomatch_output.txt");

        File.WriteAllText(testFile, "This file has no matching words", Encoding.UTF8);

        var searchWords = new[] { "notfound" };
        var files = new[] { testFile };

        var probe = new ProbeFiles(searchWords, files, true, outputFile, Encoding.UTF8);

        // Act
        var result = probe.Probe(Encoding.UTF8);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(outputFile));
    }
}
