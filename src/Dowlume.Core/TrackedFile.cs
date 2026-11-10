namespace Dowlume.Core;

public enum LifecycleStatus
{
    Organized,
    InReview,
    Kept,
    Snoozed,
    Moved,
    Recycled,
    Missing
}

public sealed record TrackedFile(
    Guid Id,
    string OriginalPath,
    string CurrentPath,
    FileCategory Category,
    DateTimeOffset FirstSeenUtc,
    LifecycleStatus Status,
    string ContentHash,
    DateTimeOffset? SnoozedUntilUtc = null)
{
    public Guid? CustomRuleId
    {
        get; init;
    }
    public string? AssignedFolder
    {
        get; init;
    }
    public int? AssignedReviewDays
    {
        get; init;
    }
    [System.Text.Json.Serialization.JsonIgnore]
    public object DisplayCategory => CustomRuleId is not null ? new FolderLabel(AssignedFolder ?? string.Empty) : Category;
    public string Name => Path.GetFileName(CurrentPath);
    public bool IsManaged => Status is not (LifecycleStatus.Moved or LifecycleStatus.Recycled or LifecycleStatus.Missing);
}
