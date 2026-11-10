namespace Dowlume.Core;

public static class FileClassifier
{
    private static readonly HashSet<string> TemporaryExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".crdownload", ".part", ".partial", ".tmp", ".temp", ".download", ".opdownload"
    };

    public static bool IsExcluded(string path, AppSettings settings)
    {
        string extension = Path.GetExtension(path);
        return TemporaryExtensions.Contains(extension) ||
               settings.ExcludedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase) ||
               Path.GetFileName(path).StartsWith('~');
    }

    public static CategoryRule Classify(string path, AppSettings settings)
    {
        string name = Path.GetFileNameWithoutExtension(path).Normalize();
        CategoryRule[] names = settings.Categories.Where(rule => rule.MatchKind switch
        {
            RuleMatchKind.NameContains => name.Contains(rule.NamePattern.Normalize(), StringComparison.OrdinalIgnoreCase),
            RuleMatchKind.NameStartsWith => name.StartsWith(rule.NamePattern.Normalize(), StringComparison.OrdinalIgnoreCase),
            _ => false
        }).ToArray();
        if (names.Length > 1)
            throw new RuleConflictException("The file {0} matches several name rules: {1}. It has not been moved. Edit the rules and preview again.",
                Path.GetFileName(path), string.Join(", ", names.Select(rule => rule.FolderName)));
        if (names.Length == 1)
            return names[0];
        CategoryRule[] extensions = settings.Categories.Where(rule => rule.MatchKind == RuleMatchKind.Extension &&
            rule.Extensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)).ToArray();
        if (extensions.Length > 1)
            throw new RuleConflictException("The extension {0} is assigned to both {1} and {2}.", Path.GetExtension(path), extensions[0].FolderName, extensions[1].FolderName);
        return extensions.SingleOrDefault() ?? settings.Categories.Single(rule => rule.Id is null && rule.Category == FileCategory.Other);
    }
}
