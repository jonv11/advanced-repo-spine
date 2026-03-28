using System.Text.Json;
using System.Text.Json.Serialization;
using Ars.Cli.Infrastructure;
using Ars.Cli.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ars.Cli.Commands;

public sealed class InitSettings : CommandSettings
{
    [CommandOption("--path")]
    public string Path { get; set; } = "ars.json";

    [CommandOption("--force")]
    public bool Force { get; set; }
}

public sealed class InitCommand : Command<InitSettings>
{
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public override int Execute(CommandContext context, InitSettings settings)
    {
        var fullPath = System.IO.Path.GetFullPath(settings.Path);

        if (File.Exists(fullPath) && !settings.Force)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] File '{Markup.Escape(fullPath)}' already exists. Use --force to overwrite.");
            return ExitCodes.InvalidInput;
        }

        var model = CreateStarterModel();
        var json = JsonSerializer.Serialize(model, WriteOptions);

        var dir = System.IO.Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(fullPath, json);

        AnsiConsole.MarkupLine($"[green]Created model file:[/] {Markup.Escape(fullPath)}");
        return ExitCodes.Success;
    }

    private static RepoModel CreateStarterModel()
    {
        return new RepoModel
        {
            Version = "1.0",
            Name = "My Repository",
            Description = "Structure model for this repository. Edit this file to define the expected layout.",
            Rules = new ModelRules
            {
                CaseSensitive = false,
                Ignore = new List<string> { ".git/", "bin/", "obj/", "node_modules/" }
            },
            Items = new List<ModelItem>
            {
                new()
                {
                    Name = "readme",
                    Type = "file",
                    Path = "README.md",
                    Required = true,
                    Description = "Project entry point for humans and AI agents."
                },
                new()
                {
                    Name = "src",
                    Type = "directory",
                    Path = "src",
                    Required = true,
                    Description = "Source code root.",
                    Children = new List<ModelItem>()
                },
                new()
                {
                    Name = "docs",
                    Type = "directory",
                    Path = "docs",
                    Required = false,
                    Description = "Documentation directory (optional).",
                    Children = new List<ModelItem>()
                }
            }
        };
    }
}
