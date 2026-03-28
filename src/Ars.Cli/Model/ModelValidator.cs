namespace Ars.Cli.Model;

public sealed record ValidationError(string Location, string Message);

public static class ModelValidator
{
    public static List<ValidationError> Validate(RepoModel model)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(model.Version))
            errors.Add(new("version", "Version is required."));
        else if (model.Version != "1.0")
            errors.Add(new("version", $"Unsupported version '{model.Version}'. Expected '1.0'."));

        if (string.IsNullOrWhiteSpace(model.Name))
            errors.Add(new("name", "Name is required."));

        if (model.Rules is null)
            errors.Add(new("rules", "Rules object is required."));

        if (model.Items is null || model.Items.Count == 0)
            errors.Add(new("items", "At least one item is required."));
        else
            ValidateItems(model.Items, "items", errors, new HashSet<string>(StringComparer.Ordinal), parentPath: null);

        return errors;
    }

    private static void ValidateItems(
        List<ModelItem> items,
        string locationPrefix,
        List<ValidationError> errors,
        HashSet<string> seenPaths,
        string? parentPath)
    {
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            string loc = $"{locationPrefix}[{i}]";

            if (string.IsNullOrWhiteSpace(item.Name))
                errors.Add(new(loc, "Item name is required."));

            if (string.IsNullOrWhiteSpace(item.Type))
                errors.Add(new(loc, "Item type is required."));
            else if (item.Type != "directory" && item.Type != "file")
                errors.Add(new(loc, $"Item type must be 'directory' or 'file', got '{item.Type}'."));

            if (string.IsNullOrWhiteSpace(item.Path))
            {
                errors.Add(new(loc, "Item path is required."));
            }
            else
            {
                if (item.Path.Contains('\\'))
                    errors.Add(new(loc, $"Path '{item.Path}' must use forward slashes only."));

                if (!seenPaths.Add(item.Path))
                    errors.Add(new(loc, $"Duplicate path '{item.Path}'."));

                if (parentPath != null && !item.Path.StartsWith(parentPath + "/", StringComparison.Ordinal))
                    errors.Add(new(loc, $"Child path '{item.Path}' must be prefixed by parent path '{parentPath}/'."));
            }

            if (item.Children != null && item.Children.Count > 0)
            {
                if (item.Type == "file")
                    errors.Add(new(loc, "File items cannot have children."));

                ValidateItems(item.Children, $"{loc}.children", errors, seenPaths, item.Path);
            }
        }
    }
}
