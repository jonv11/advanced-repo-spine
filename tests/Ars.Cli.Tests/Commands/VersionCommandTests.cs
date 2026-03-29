using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Ars.Cli.Tests.Commands;

public sealed class VersionCommandTests : IDisposable
{
    private readonly string _tempDir;

    public VersionCommandTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ars-version-tests-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task Version_CurrentBuild_PrintsSemVerMetadataWithShortGitHash()
    {
        var repoRoot = FindRepositoryRoot();
        var sourceRevisionId = (await RunProcessAsync("git", ["rev-parse", "HEAD"], repoRoot)).RequireSuccess().StdOut.Trim();
        var expectedHash = (await RunProcessAsync("git", ["rev-parse", "--short", "HEAD"], repoRoot)).RequireSuccess().StdOut.Trim();
        var cliDllPath = await BuildCliAsync(repoRoot, sourceRevisionId);

        var version = (await RunProcessAsync("dotnet", [cliDllPath, "--version"], repoRoot)).RequireSuccess().StdOut.Trim();

        Assert.Equal($"1.0.0+sha.{expectedHash}", version);
        Assert.Matches(new Regex(@"^1\.0\.0\+sha\.[0-9a-f]{7,}$"), version);
    }

    [Fact]
    public async Task Version_FallsBackToPlainReleaseVersion_WhenSourceRevisionIdMissing()
    {
        var repoRoot = FindRepositoryRoot();
        var cliDllPath = await BuildCliAsync(repoRoot, sourceRevisionId: string.Empty);

        var version = (await RunProcessAsync("dotnet", [cliDllPath, "--version"], repoRoot)).RequireSuccess().StdOut.Trim();

        Assert.Equal("1.0.0", version);
    }

    private async Task<string> BuildCliAsync(string repoRoot, string? sourceRevisionId = null)
    {
        var buildRoot = Path.Combine(_tempDir, Guid.NewGuid().ToString("N"));
        var projectCopyRoot = Path.Combine(buildRoot, "Ars.Cli");
        var sourceProjectRoot = Path.Combine(repoRoot, "src", "Ars.Cli");
        var projectPath = Path.Combine(projectCopyRoot, "Ars.Cli.csproj");

        CopyDirectory(sourceProjectRoot, projectCopyRoot);

        var arguments = new List<string>
        {
            "build",
            projectPath,
            "-c",
            "Release",
            "-nologo",
            "-p:UseSharedCompilation=false"
        };

        if (sourceRevisionId is not null)
            arguments.Add($"-p:SourceRevisionId={sourceRevisionId}");

        (await RunProcessAsync("dotnet", arguments, projectCopyRoot, timeoutMs: 120000)).RequireSuccess();

        var cliDllPath = Path.Combine(projectCopyRoot, "bin", "Release", "net8.0", "ars.dll");
        Assert.True(File.Exists(cliDllPath), $"Expected CLI assembly at '{cliDllPath}'.");
        return cliDllPath;
    }

    private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);

        foreach (var filePath in Directory.EnumerateFiles(sourceDirectory))
        {
            var destinationPath = Path.Combine(destinationDirectory, Path.GetFileName(filePath));
            File.Copy(filePath, destinationPath, overwrite: true);
        }

        foreach (var directoryPath in Directory.EnumerateDirectories(sourceDirectory))
        {
            var directoryName = Path.GetFileName(directoryPath);

            if (directoryName is "bin" or "obj")
                continue;

            CopyDirectory(directoryPath, Path.Combine(destinationDirectory, directoryName));
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ars.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root from the test output directory.");
    }

    private static async Task<ProcessResult> RunProcessAsync(
        string fileName,
        IEnumerable<string> arguments,
        string workingDirectory,
        int timeoutMs = 30000)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        foreach (var argument in arguments)
            process.StartInfo.ArgumentList.Add(argument);

        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        using var cancellation = new CancellationTokenSource(timeoutMs);

        try
        {
            await process.WaitForExitAsync(cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
                // The process exited between the timeout and the kill attempt.
            }

            throw new TimeoutException($"Process '{fileName}' timed out after {timeoutMs} ms.");
        }

        return new ProcessResult(process.ExitCode, await stdoutTask, await stderrTask);
    }

    private sealed record ProcessResult(int ExitCode, string StdOut, string StdErr)
    {
        public ProcessResult RequireSuccess()
        {
            Assert.True(
                ExitCode == 0,
                $"Expected process to succeed but got exit code {ExitCode}.{Environment.NewLine}STDOUT:{Environment.NewLine}{StdOut}{Environment.NewLine}STDERR:{Environment.NewLine}{StdErr}");

            return this;
        }
    }
}
