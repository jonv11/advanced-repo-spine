using System.ComponentModel;
using Spectre.Console.Cli;

namespace Ars.Cli.Commands;

public sealed class ExportSettings : CommandSettings
{
    [CommandOption("--model")]
    [Description("Path to the JSON model file [default: ars.json]")]
    public string Model { get; set; } = "ars.json";

    [CommandOption("--root")]
    [Description("Root directory to compare against [default: .]")]
    public string Root { get; set; } = ".";
}

public sealed class ExportCommand : Command<ExportSettings>
{
    public override int Execute(CommandContext context, ExportSettings settings)
    {
        return CompareCommand.ExecuteCompare(settings.Model, settings.Root, "json");
    }
}
