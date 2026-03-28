using Ars.Cli.Model;

namespace Ars.Cli.Suggestion;

public static class SuggestionEngine
{
    public static SuggestionResult Suggest(RepoModel model, string input)
    {
        var normalizedInput = input.Replace('\\', '/');
        var suggestions = new List<Suggestion>();

        var allItems = new List<ModelItem>();
        FlattenItems(model.Items, allItems);

        var comparison = model.Rules.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        // 1. Exact path match
        var exactMatch = allItems.FirstOrDefault(i => i.Path.Equals(normalizedInput, comparison));
        if (exactMatch != null)
        {
            suggestions.Add(new Suggestion
            {
                Path = exactMatch.Path,
                Reason = $"Exact match: '{exactMatch.Path}' is defined in the model as '{exactMatch.Name}'.",
                Confidence = "high"
            });

            return new SuggestionResult
            {
                Input = input,
                Suggestions = suggestions,
                Message = $"'{input}' matches an existing model entry."
            };
        }

        // 2. Parent directory match — find the deepest model directory that would contain this path
        var parentMatches = allItems
            .Where(i => i.Type == "directory" && normalizedInput.StartsWith(i.Path + "/", comparison))
            .OrderByDescending(i => i.Path.Length)
            .ToList();

        foreach (var parent in parentMatches)
        {
            suggestions.Add(new Suggestion
            {
                Path = parent.Path,
                Reason = $"'{input}' would belong under '{parent.Path}' ({parent.Name}).",
                Confidence = parentMatches[0] == parent ? "medium" : "low"
            });
        }

        // 3. Filename match — find model items whose filename matches
        var inputFilename = GetFilename(normalizedInput);
        if (!string.IsNullOrEmpty(inputFilename))
        {
            var filenameMatches = allItems
                .Where(i => GetFilename(i.Path).Equals(inputFilename, comparison))
                .ToList();

            foreach (var match in filenameMatches)
            {
                // Avoid duplicates with parent matches
                if (suggestions.Any(s => s.Path == match.Path))
                    continue;

                suggestions.Add(new Suggestion
                {
                    Path = match.Path,
                    Reason = $"Filename '{inputFilename}' matches model item '{match.Path}' ({match.Name}).",
                    Confidence = filenameMatches.Count == 1 ? "medium" : "low"
                });
            }
        }

        // 4. Name similarity — find model items whose name contains the input filename (or vice versa)
        if (!string.IsNullOrEmpty(inputFilename))
        {
            var nameMatches = allItems
                .Where(i =>
                    i.Name.Contains(inputFilename, comparison) ||
                    inputFilename.Contains(i.Name, comparison))
                .Where(i => !suggestions.Any(s => s.Path == i.Path))
                .ToList();

            foreach (var match in nameMatches)
            {
                suggestions.Add(new Suggestion
                {
                    Path = match.Path,
                    Reason = $"Name similarity: '{inputFilename}' is related to '{match.Name}' at '{match.Path}'.",
                    Confidence = "low"
                });
            }
        }

        if (suggestions.Count == 0)
        {
            return new SuggestionResult
            {
                Input = input,
                Suggestions = suggestions,
                Message = $"No suggestion available for '{input}'. The model does not contain a matching entry."
            };
        }

        return new SuggestionResult
        {
            Input = input,
            Suggestions = suggestions,
            Message = $"Found {suggestions.Count} suggestion(s) for '{input}'."
        };
    }

    private static void FlattenItems(List<ModelItem> items, List<ModelItem> result)
    {
        foreach (var item in items)
        {
            result.Add(item);
            if (item.Children != null)
                FlattenItems(item.Children, result);
        }
    }

    private static string GetFilename(string path)
    {
        int lastSlash = path.LastIndexOf('/');
        return lastSlash >= 0 ? path[(lastSlash + 1)..] : path;
    }
}
