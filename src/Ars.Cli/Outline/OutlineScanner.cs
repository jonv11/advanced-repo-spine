namespace Ars.Cli.Outline;

public static class OutlineScanner
{
    public static OutlineNode Scan(string path, int? maxDepth, int? headingsDepth)
    {
        var fullPath = Path.GetFullPath(path);

        if (File.Exists(fullPath))
            return ScanFile(fullPath, Path.GetDirectoryName(fullPath)!, headingsDepth);

        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException($"Path not found: {path}");

        return ScanDirectory(fullPath, fullPath, 0, maxDepth, headingsDepth);
    }

    private static OutlineNode ScanDirectory(
        string dirPath,
        string rootPath,
        int currentDepth,
        int? maxDepth,
        int? headingsDepth)
    {
        var name = dirPath == rootPath
            ? Path.GetFileName(dirPath)
            : Path.GetFileName(dirPath);

        var relativePath = NormalizePath(Path.GetRelativePath(rootPath, dirPath));
        if (relativePath == ".")
            relativePath = Path.GetFileName(dirPath);

        var children = new List<OutlineNode>();

        if (!maxDepth.HasValue || currentDepth < maxDepth.Value)
        {
            try
            {
                var directories = Directory.GetDirectories(dirPath);
                Array.Sort(directories, StringComparer.OrdinalIgnoreCase);

                foreach (var subDir in directories)
                {
                    children.Add(ScanDirectory(
                        subDir, rootPath, currentDepth + 1, maxDepth, headingsDepth));
                }

                var files = Directory.GetFiles(dirPath);
                Array.Sort(files, StringComparer.OrdinalIgnoreCase);

                foreach (var file in files)
                {
                    children.Add(ScanFile(file, rootPath, headingsDepth));
                }
            }
            catch
            {
                return new OutlineNode
                {
                    Type = OutlineNodeType.Directory,
                    Name = name,
                    Path = relativePath,
                    Error = true,
                    Children = children
                };
            }
        }

        return new OutlineNode
        {
            Type = OutlineNodeType.Directory,
            Name = name,
            Path = relativePath,
            Children = children
        };
    }

    private static OutlineNode ScanFile(string filePath, string rootPath, int? headingsDepth)
    {
        var name = Path.GetFileName(filePath);
        var relativePath = NormalizePath(Path.GetRelativePath(rootPath, filePath));

        var isMarkdown = name.EndsWith(".md", StringComparison.OrdinalIgnoreCase);

        List<OutlineNode> headings;
        var error = false;

        if (isMarkdown)
        {
            try
            {
                headings = HeadingExtractor.Extract(filePath, headingsDepth);
            }
            catch
            {
                headings = [];
                error = true;
            }
        }
        else
        {
            headings = [];
        }

        return new OutlineNode
        {
            Type = OutlineNodeType.File,
            Name = name,
            Path = relativePath,
            Error = error,
            Children = headings
        };
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }
}
