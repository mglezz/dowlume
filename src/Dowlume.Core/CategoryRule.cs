namespace Dowlume.Core;

public sealed record CategoryRule(FileCategory Category, string FolderName, string[] Extensions, int ReviewAfterDays)
{
    public Guid? Id
    {
        get; init;
    }
    public RuleMatchKind MatchKind
    {
        get; init;
    }
    public string NamePattern { get; init; } = string.Empty;

    public bool HasSameValuesAs(CategoryRule other) =>
        Id == other.Id && MatchKind == other.MatchKind && NamePattern == other.NamePattern &&
        Category == other.Category && FolderName == other.FolderName &&
        ReviewAfterDays == other.ReviewAfterDays && Extensions.SequenceEqual(other.Extensions);
}

public enum RuleMatchKind
{
    Extension, NameContains, NameStartsWith
}

// Folder names are user text, never translation keys.
public sealed record FolderLabel(string Name);

public sealed class RuleConflictException(string message, params string[] details) : ArgumentException(message)
{
    public string[] Details { get; } = details;
}
