using Spectre.Console;

namespace Ars.Cli.Outline;

public static class OutlineTextRenderer
{
    public static void Render(OutlineNode root)
    {
        var tree = new Tree(FormatNode(root));
        AddChildren(tree, root);
        AnsiConsole.Write(tree);
    }

    private static void AddChildren(IHasTreeNodes parent, OutlineNode node)
    {
        foreach (var child in node.Children)
        {
            var branch = parent.AddNode(FormatNode(child));
            AddChildren(branch, child);
        }
    }

    private static string FormatNode(OutlineNode node)
    {
        return node.Type switch
        {
            OutlineNodeType.Directory => node.Error
                ? $"[bold]{Markup.Escape(node.Name)}/[/] [dim](unreadable)[/]"
                : $"[bold]{Markup.Escape(node.Name)}/[/]",

            OutlineNodeType.File => node.Error
                ? $"{Markup.Escape(node.Name)} [dim](unreadable)[/]"
                : Markup.Escape(node.Name),

            OutlineNodeType.Heading =>
                $"{new string('#', node.Level!.Value)} {Markup.Escape(node.Name)}",

            _ => Markup.Escape(node.Name)
        };
    }
}
