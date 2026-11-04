namespace Dowlume.Core;

public sealed record CategoryRule(FileCategory Category, string FolderName, string[] Extensions, int ReviewAfterDays)
{
    public bool HasSameValuesAs(CategoryRule other) =>
        Category == other.Category && FolderName == other.FolderName &&
        ReviewAfterDays == other.ReviewAfterDays && Extensions.SequenceEqual(other.Extensions);
}
