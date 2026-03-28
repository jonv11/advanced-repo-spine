using Spectre.Console.Cli;

namespace Ars.Cli.Commands;

public sealed class ReportSettings : CommandSettings
{
    [CommandOption("--model")]
    public string Model { get; set; } = "ars.json";

    [CommandOption("--root")]
    public string Root { get; set; } = ".";
}

public sealed class ReportCommand : Command<ReportSettings>
{
    public override int Execute(CommandContext context, ReportSettings settings)
    {
        return CompareCommand.ExecuteCompare(settings.Model, settings.Root, "text");
    }
}
