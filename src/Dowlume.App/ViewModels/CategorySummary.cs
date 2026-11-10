using Dowlume.Core;

namespace Dowlume.App.ViewModels;

public sealed record CategorySummary(FileCategory Category, int Count, string? CustomFolder = null)
{
    public object DisplayCategory => CustomFolder is not null ? new FolderLabel(CustomFolder) : Category;
}
