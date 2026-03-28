using Ars.Cli.Model;
using Ars.Cli.Scanning;

namespace Ars.Cli.Tests.Scanning;

public class FileSystemScannerTests : IDisposable
{
    private readonly string _tempDir;

    public FileSystemScannerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ars_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void Scan_EmptyDirectory_ReturnsEmpty()
    {
        var rules = new ModelRules { CaseSensitive = false, Ignore = new List<string>() };

        var results = FileSystemScanner.Scan(_tempDir, rules);

        Assert.Empty(results);
    }

    [Fact]
    public void Scan_SingleFile_ReturnsFileItem()
    {
        File.WriteAllText(Path.Combine(_tempDir, "README.md"), "content");
        var rules = new ModelRules { CaseSensitive = false, Ignore = new List<string>() };

        var results = FileSystemScanner.Scan(_tempDir, rules);

        Assert.Single(results);
        Assert.Equal("README.md", results[0].Path);
        Assert.Equal("file", results[0].Type);
    }

    [Fact]
    public void Scan_DirectoryAndFiles_ReturnsAll()
    {
        var subDir = Path.Combine(_tempDir, "src");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "main.cs"), "content");
        var rules = new ModelRules { CaseSensitive = false, Ignore = new List<string>() };

        var results = FileSystemScanner.Scan(_tempDir, rules);

        Assert.Equal(2, results.Count); // src/ and src/main.cs
        Assert.Contains(results, r => r.Path == "src" && r.Type == "directory");
        Assert.Contains(results, r => r.Path == "src/main.cs" && r.Type == "file");
    }

    [Fact]
    public void Scan_PathsUseForwardSlashes()
    {
        var subDir = Path.Combine(_tempDir, "docs", "guides");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "guide.md"), "content");
        var rules = new ModelRules { CaseSensitive = false, Ignore = new List<string>() };

        var results = FileSystemScanner.Scan(_tempDir, rules);

        foreach (var item in results)
        {
            Assert.DoesNotContain("\\", item.Path);
        }
        Assert.Contains(results, r => r.Path == "docs/guides/guide.md");
    }

    [Fact]
    public void Scan_IgnoreDirectoryPattern_ExcludesMatchingDirectory()
    {
        var binDir = Path.Combine(_tempDir, "bin");
        Directory.CreateDirectory(binDir);
        File.WriteAllText(Path.Combine(binDir, "app.dll"), "binary");
        File.WriteAllText(Path.Combine(_tempDir, "README.md"), "content");
        var rules = new ModelRules
        {
            CaseSensitive = false,
            Ignore = new List<string> { "bin/" }
        };

        var results = FileSystemScanner.Scan(_tempDir, rules);

        Assert.DoesNotContain(results, r => r.Path.StartsWith("bin"));
        Assert.Contains(results, r => r.Path == "README.md");
    }

    [Fact]
    public void Scan_IgnoreFilePattern_ExcludesMatchingFile()
    {
        File.WriteAllText(Path.Combine(_tempDir, "README.md"), "content");
        File.WriteAllText(Path.Combine(_tempDir, ".gitignore"), "stuff");
        var rules = new ModelRules
        {
            CaseSensitive = false,
            Ignore = new List<string> { ".gitignore" }
        };

        var results = FileSystemScanner.Scan(_tempDir, rules);

        Assert.DoesNotContain(results, r => r.Path == ".gitignore");
        Assert.Contains(results, r => r.Path == "README.md");
    }

    [Fact]
    public void Scan_DeterministicOrdering_SortedByPath()
    {
        File.WriteAllText(Path.Combine(_tempDir, "z.txt"), "");
        File.WriteAllText(Path.Combine(_tempDir, "a.txt"), "");
        File.WriteAllText(Path.Combine(_tempDir, "m.txt"), "");
        var rules = new ModelRules { CaseSensitive = false, Ignore = new List<string>() };

        var results = FileSystemScanner.Scan(_tempDir, rules);

        var paths = results.Select(r => r.Path).ToList();
        var sorted = paths.OrderBy(p => p, StringComparer.Ordinal).ToList();
        Assert.Equal(sorted, paths);
    }
}
