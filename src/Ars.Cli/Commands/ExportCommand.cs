using Spectre.Console.Cli;

namespace Ars.Cli.Commands;

public sealed class ExportSettings : CommandSettings
{
    [CommandOption("--model")]
    public string Model { get; set; } = "ars.json";

    [CommandOption("--root")]
    public string Root { get; set; } = ".";
}

public sealed class ExportCommand : Command<ExportSettings>
{
    public override int Execute(CommandContext context, ExportSettings settings)
    {
        return CompareCommand.ExecuteCompare(settings.Model, settings.Root, "json");
    }
}
