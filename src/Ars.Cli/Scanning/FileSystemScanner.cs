using Ars.Cli.Model;

namespace Ars.Cli.Scanning;

public static class FileSystemScanner
{
    public static List<ScannedItem> Scan(string rootPath, ModelRules rules)
    {
        var fullRoot = Path.GetFullPath(rootPath);
        if (!Directory.Exists(fullRoot))
            throw new DirectoryNotFoundException($"Root directory not found: {rootPath}");

        var items = new List<ScannedItem>();
        ScanDirectory(fullRoot, fullRoot, rules.Ignore, items);
        items.Sort((a, b) => string.Compare(a.Path, b.Path, StringComparison.Ordinal));
        return items;
    }

    private static void ScanDirectory(
        string currentDir,
        string rootDir,
        List<string> ignorePatterns,
        List<ScannedItem> items)
    {
        foreach (var dir in Directory.GetDirectories(currentDir))
        {
            var relativePath = NormalizePath(Path.GetRelativePath(rootDir, dir));
            if (IsIgnored(relativePath, ignorePatterns))
                continue;

            items.Add(new ScannedItem { Path = relativePath, Type = "directory" });
            ScanDirectory(dir, rootDir, ignorePatterns, items);
        }

        foreach (var file in Directory.GetFiles(currentDir))
        {
            var relativePath = NormalizePath(Path.GetRelativePath(rootDir, file));
            if (IsIgnored(relativePath, ignorePatterns))
                continue;

            items.Add(new ScannedItem { Path = relativePath, Type = "file" });
        }
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    private static bool IsIgnored(string path, List<string> ignorePatterns)
    {
        foreach (var pattern in ignorePatterns)
        {
            if (pattern.EndsWith('/'))
            {
                // Directory prefix match: ignore if path starts with the pattern (without trailing slash)
                var prefix = pattern.TrimEnd('/');
                if (path.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            else
            {
                // Exact match
                if (path.Equals(pattern, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        return false;
    }
}
