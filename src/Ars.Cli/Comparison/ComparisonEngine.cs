using Ars.Cli.Model;
using Ars.Cli.Scanning;

namespace Ars.Cli.Comparison;

public static class ComparisonEngine
{
    public static ComparisonResult Compare(RepoModel model, List<ScannedItem> scannedItems, string root)
    {
        var findings = new List<Finding>();
        var comparison = model.Rules.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        // 1. Flatten model hierarchy into a list of all expected items
        var expectedItems = new List<ModelItem>();
        FlattenItems(model.Items, expectedItems);

        // 2. Build a set of scanned paths for fast lookup
        var scannedByPath = new Dictionary<string, ScannedItem>(
            model.Rules.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
        foreach (var item in scannedItems)
            scannedByPath.TryAdd(item.Path, item);

        // Track which scanned items were matched
        var matchedScanPaths = new HashSet<string>(
            model.Rules.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

        // Track missing items for misplacement detection
        var missingItems = new List<ModelItem>();

        // 3. For each model item, check if present in scan
        foreach (var expected in expectedItems)
        {
            if (scannedByPath.ContainsKey(expected.Path))
            {
                matchedScanPaths.Add(expected.Path);
                findings.Add(new Finding
                {
                    Type = FindingType.Present,
                    Severity = FindingSeverity.Info,
                    ExpectedPath = expected.Path,
                    ActualPath = expected.Path,
                    ItemName = expected.Name,
                    Message = $"Expected {expected.Type} is present."
                });
            }
            else if (expected.Required)
            {
                missingItems.Add(expected);
            }
            else
            {
                findings.Add(new Finding
                {
                    Type = FindingType.OptionalMissing,
                    Severity = FindingSeverity.Info,
                    ExpectedPath = expected.Path,
                    ActualPath = null,
                    ItemName = expected.Name,
                    Message = $"Optional {expected.Type} '{expected.Path}' is not present."
                });
            }
        }

        // 4. For each unmatched scanned item, attempt conservative misplacement detection
        var unmatchedScanned = scannedItems
            .Where(s => !matchedScanPaths.Contains(s.Path))
            .ToList();

        // Build a map of filename → missing model items for misplacement
        var missingByFilename = new Dictionary<string, List<ModelItem>>(
            model.Rules.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

        foreach (var missing in missingItems)
        {
            var filename = GetFilename(missing.Path);
            if (!missingByFilename.TryGetValue(filename, out var list))
            {
                list = new List<ModelItem>();
                missingByFilename[filename] = list;
            }
            list.Add(missing);
        }

        var claimedMissingPaths = new HashSet<string>(
            model.Rules.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

        foreach (var scanned in unmatchedScanned)
        {
            var filename = GetFilename(scanned.Path);

            if (missingByFilename.TryGetValue(filename, out var candidates))
            {
                // Filter to candidates not already claimed
                var available = candidates.Where(c => !claimedMissingPaths.Contains(c.Path)).ToList();

                if (available.Count == 1)
                {
                    var match = available[0];
                    claimedMissingPaths.Add(match.Path);
                    findings.Add(new Finding
                    {
                        Type = FindingType.Misplaced,
                        Severity = FindingSeverity.Warning,
                        ExpectedPath = match.Path,
                        ActualPath = scanned.Path,
                        ItemName = match.Name,
                        Message = $"'{scanned.Path}' appears to be '{match.Path}' in the wrong location."
                    });
                    continue;
                }
            }

            findings.Add(new Finding
            {
                Type = FindingType.Unmatched,
                Severity = FindingSeverity.Warning,
                ExpectedPath = null,
                ActualPath = scanned.Path,
                ItemName = null,
                Message = $"'{scanned.Path}' is not defined in the model."
            });
        }

        // Emit Missing findings for items not claimed by misplacement
        foreach (var missing in missingItems)
        {
            if (!claimedMissingPaths.Contains(missing.Path))
            {
                findings.Add(new Finding
                {
                    Type = FindingType.Missing,
                    Severity = FindingSeverity.Error,
                    ExpectedPath = missing.Path,
                    ActualPath = null,
                    ItemName = missing.Name,
                    Message = $"Required {missing.Type} '{missing.Path}' is missing."
                });
            }
        }

        // 5. Sort findings: by type ordinal, then by path
        findings.Sort((a, b) =>
        {
            int typeCompare = a.Type.CompareTo(b.Type);
            if (typeCompare != 0) return typeCompare;
            var pathA = a.ExpectedPath ?? a.ActualPath ?? string.Empty;
            var pathB = b.ExpectedPath ?? b.ActualPath ?? string.Empty;
            return string.Compare(pathA, pathB, StringComparison.Ordinal);
        });

        var summary = new ComparisonSummary
        {
            Missing = findings.Count(f => f.Type == FindingType.Missing),
            Present = findings.Count(f => f.Type == FindingType.Present),
            OptionalMissing = findings.Count(f => f.Type == FindingType.OptionalMissing),
            Unmatched = findings.Count(f => f.Type == FindingType.Unmatched),
            Misplaced = findings.Count(f => f.Type == FindingType.Misplaced),
            Total = findings.Count
        };

        return new ComparisonResult
        {
            ModelVersion = model.Version,
            ModelName = model.Name,
            Root = root,
            Timestamp = DateTimeOffset.UtcNow,
            Settings = new ComparisonSettings
            {
                CaseSensitive = model.Rules.CaseSensitive,
                IgnoredPatterns = model.Rules.Ignore
            },
            Summary = summary,
            Findings = findings
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
