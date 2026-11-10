namespace Dowlume.Core;

public sealed record AppSettings
{
    public const string ReviewFolderName = "Expiring Files";

    public string Language { get; init; } = "en";

    public bool ShowWindowAtWindowsLogin
    {
        get; init;
    }

    public string DownloadsPath { get; init; } = string.Empty;
    public bool UsePeriodicOrganization
    {
        get; init;
    }
    public int OrganizationIntervalMinutes { get; init; } = 10;
    public List<CategoryRule> Categories
    {
        get; init;
    } =
    [
        new(FileCategory.Documents, "Documents", [".pdf", ".txt", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".csv", ".odt", ".epub"], 30),
        new(FileCategory.Images, "Images", [".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".heic", ".bmp", ".tiff"], 60),
        new(FileCategory.Videos, "Videos", [".mp4", ".mkv", ".mov", ".avi", ".webm"], 30),
        new(FileCategory.Archives, "Archives", [".zip", ".7z", ".rar", ".tar", ".gz"], 14),
        new(FileCategory.Installers, "Installers", [".exe", ".msi", ".msix", ".appx", ".iso", ".dmg", ".pkg"], 7),
        new(FileCategory.Other, "Other", [], 30)
    ];
    public string[] ExcludedExtensions { get; init; } = [];

    // Preview consent compares values, including collection order and case, not record references.
    public bool HasSameValuesAs(AppSettings? other) =>
        other is not null && Language == other.Language && DownloadsPath == other.DownloadsPath &&
        UsePeriodicOrganization == other.UsePeriodicOrganization && OrganizationIntervalMinutes == other.OrganizationIntervalMinutes &&
        ExcludedExtensions.SequenceEqual(other.ExcludedExtensions) && Categories.Count == other.Categories.Count &&
        Categories.Zip(other.Categories).All(pair => pair.First.HasSameValuesAs(pair.Second));

    public void Validate()
    {
        if (OrganizationIntervalMinutes is < 1 or > 1440)
        {
            throw new ArgumentException("The organization interval must be 1–1440 minutes.");
        }
        if (!Path.IsPathFullyQualified(DownloadsPath))
        {
            throw new ArgumentException("Choose an absolute Downloads folder path.");
        }

        CategoryRule[] builtIn = Categories.Where(rule => rule.Id is null).ToArray();
        if (builtIn.Length != Enum.GetValues<FileCategory>().Length ||
            builtIn.Select(rule => rule.Category).Distinct().Count() != builtIn.Length ||
            Categories.Any(rule => !Enum.IsDefined(rule.Category)) ||
            Categories.Any(rule => rule.Id is not null && (rule.Id == Guid.Empty || rule.Category != FileCategory.Other)) ||
            Categories.Where(rule => rule.Id is not null).Select(rule => rule.Id).Distinct().Count() != Categories.Count - builtIn.Length)
        {
            throw new ArgumentException("Define exactly one rule for each category.");
        }
        if (Categories.Count > 56)
            throw new ArgumentException("You can create up to 50 custom folders.");

        var folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ReviewFolderName };
        var extensions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (CategoryRule rule in Categories)
        {
            ValidateFolderName(rule.FolderName);
            if (!folders.Add(rule.FolderName.Normalize()) || rule.ReviewAfterDays is < 1 or > 3650)
            {
                throw new ArgumentException("Category folders must be unique; review periods must be 1–3650 days.");
            }
            if (!Enum.IsDefined(rule.MatchKind) ||
                (rule.Id is null && rule.Category == FileCategory.Other && rule.MatchKind != RuleMatchKind.Extension))
                throw new ArgumentException("The Other folder must remain the extension fallback.");
            if (rule.MatchKind != RuleMatchKind.Extension)
            {
                if (string.IsNullOrWhiteSpace(rule.NamePattern) || rule.NamePattern.Length > 100 ||
                    rule.NamePattern != rule.NamePattern.Trim() || rule.NamePattern.Any(char.IsControl) ||
                    rule.NamePattern.IndexOfAny(['/', '\\', '*', '?']) >= 0 || rule.Extensions.Length != 0)
                    throw new ArgumentException("Name rules need a literal pattern of 1–100 characters, without wildcards, paths or extensions.");
                string pattern = rule.NamePattern.Normalize();
                if (names.TryGetValue(pattern, out string? owner))
                    throw new RuleConflictException("The name pattern {0} is assigned to both {1} and {2}.", pattern, owner, rule.FolderName);
                names.Add(pattern, rule.FolderName);
            }
            else if (rule.NamePattern.Length != 0 || (rule.Id is not null && rule.Extensions.Length == 0))
                throw new ArgumentException("Extension rules need extensions and no name pattern. Built-in folders may have no extensions.");
            foreach (string extension in rule.Extensions)
            {
                ValidateExtension(extension);
                if (extensions.TryGetValue(extension, out string? owner))
                {
                    throw new RuleConflictException("The extension {0} is assigned to both {1} and {2}.", extension, owner, rule.FolderName);
                }
                extensions.Add(extension, rule.FolderName);
            }
        }
        foreach (string extension in ExcludedExtensions)
        {
            ValidateExtension(extension);
        }
    }

    public CategoryRule RuleFor(TrackedFile file)
    {
        if (file.CustomRuleId is not { } id)
            return Categories.Single(rule => rule.Id is null && rule.Category == file.Category);
        CategoryRule? current = Categories.SingleOrDefault(rule => rule.Id == id);
        if (current is not null)
            return current;
        // Removing a rule never strands its existing review items or changes their identity.
        ValidateFolderName(file.AssignedFolder ?? string.Empty);
        if (file.AssignedReviewDays is not (>= 1 and <= 3650))
            throw new ArgumentException("The saved custom folder is unavailable. Choose Move to instead.");
        return new CategoryRule(FileCategory.Other, file.AssignedFolder!, [], file.AssignedReviewDays.Value) { Id = id };
    }

    private static void ValidateFolderName(string name)
    {
        string stem = name.Split('.')[0];
        string[] reserved = ["CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"];
        if (string.IsNullOrWhiteSpace(name) || name.Length > 80 || name is "." or ".." ||
            name.IndexOfAny(['<', '>', ':', '"', '/', '\\', '|', '?', '*']) >= 0 ||
            name.Any(char.IsControl) || name.EndsWith('.') || name.EndsWith(' ') ||
            reserved.Contains(stem, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Category names must be portable folder names.");
        }
    }

    private static void ValidateExtension(string extension)
    {
        if (extension.Length < 2 || extension[0] != '.' || !extension.Skip(1).All(char.IsAsciiLetterOrDigit))
        {
            throw new ArgumentException("Extensions must start with a dot and contain only letters and numbers.");
        }
    }
}
