using Ars.Cli.Commands;
using Ars.Cli.Infrastructure;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("ars");
    config.AddCommand<InitCommand>("init")
        .WithDescription("Create a starter JSON model file.");
    config.AddCommand<ValidateCommand>("validate")
        .WithDescription("Validate a JSON model file.");
    config.AddCommand<CompareCommand>("compare")
        .WithDescription("Compare repository structure against the model.");
    config.AddCommand<ReportCommand>("report")
        .WithDescription("Display comparison results (alias for compare --format text).");
    config.AddCommand<SuggestCommand>("suggest")
        .WithDescription("Suggest where a path belongs according to the model.");
    config.AddCommand<ExportCommand>("export")
        .WithDescription("Export comparison results as JSON (alias for compare --format json).");
});

try
{
    return app.Run(args);
}
catch (Exception)
{
    return ExitCodes.InternalError;
}
