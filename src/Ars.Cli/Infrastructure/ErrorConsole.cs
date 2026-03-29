using Spectre.Console;

namespace Ars.Cli.Infrastructure;

public static class ErrorConsole
{
    private static readonly IAnsiConsole Instance = AnsiConsole.Create(
        new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(Console.Error)
        });

    public static IAnsiConsole Stderr => Instance;
}
