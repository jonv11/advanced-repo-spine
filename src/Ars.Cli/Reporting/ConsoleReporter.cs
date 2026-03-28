using Ars.Cli.Comparison;
using Spectre.Console;

namespace Ars.Cli.Reporting;

public static class ConsoleReporter
{
    public static void Render(ComparisonResult result)
    {
        // Header
        AnsiConsole.MarkupLine($"[bold]Model:[/] {Markup.Escape(result.ModelName)} (v{Markup.Escape(result.ModelVersion)})");
        AnsiConsole.MarkupLine($"[bold]Root:[/]  {Markup.Escape(result.Root)}");
        AnsiConsole.MarkupLine($"[bold]Time:[/]  {result.Timestamp:yyyy-MM-dd HH:mm:ss} UTC");
        AnsiConsole.WriteLine();

        // Summary table
        var table = new Table();
        table.AddColumn("Category");
        table.AddColumn(new TableColumn("Count").RightAligned());

        AddSummaryRow(table, "Present", result.Summary.Present, "green");
        AddSummaryRow(table, "Missing", result.Summary.Missing, result.Summary.Missing > 0 ? "red" : "green");
        AddSummaryRow(table, "Misplaced", result.Summary.Misplaced, result.Summary.Misplaced > 0 ? "yellow" : "green");
        AddSummaryRow(table, "Unmatched", result.Summary.Unmatched, result.Summary.Unmatched > 0 ? "yellow" : "green");
        AddSummaryRow(table, "Optional Missing", result.Summary.OptionalMissing, "dim");
        table.AddRow(new Markup("[bold]Total[/]"), new Markup($"[bold]{result.Summary.Total}[/]"));

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        // Findings by category
        RenderFindingGroup(result, FindingType.Missing, "Missing (Required)", "red");
        RenderFindingGroup(result, FindingType.Misplaced, "Misplaced", "yellow");
        RenderFindingGroup(result, FindingType.Unmatched, "Unmatched", "yellow");
        RenderFindingGroup(result, FindingType.OptionalMissing, "Optional Missing", "dim");

        // Footer
        bool hasErrors = result.Summary.Missing > 0 || result.Summary.Misplaced > 0;
        if (hasErrors)
            AnsiConsole.MarkupLine("[red bold]Result: FAIL[/] — structural issues detected.");
        else
            AnsiConsole.MarkupLine("[green bold]Result: PASS[/] — no structural issues.");
    }

    private static void AddSummaryRow(Table table, string label, int count, string color)
    {
        table.AddRow(new Markup(label), new Markup($"[{color}]{count}[/]"));
    }

    private static void RenderFindingGroup(ComparisonResult result, FindingType type, string heading, string color)
    {
        var findings = result.Findings.Where(f => f.Type == type).ToList();
        if (findings.Count == 0)
            return;

        AnsiConsole.MarkupLine($"[{color} bold]{Markup.Escape(heading)}[/] ({findings.Count}):");
        foreach (var finding in findings)
        {
            var path = finding.ExpectedPath ?? finding.ActualPath ?? "(unknown)";
            AnsiConsole.MarkupLine($"  [{color}]•[/] {Markup.Escape(finding.Message)}");
        }
        AnsiConsole.WriteLine();
    }
}
