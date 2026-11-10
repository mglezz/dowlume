namespace Dowlume.Core;

public sealed record PreviewItem(string SourcePath, string DestinationPath, FileCategory Category, string ContentHash)
{
    public string? CustomFolder
    {
        get; init;
    }
    public object DisplayCategory => CustomFolder is not null ? new FolderLabel(CustomFolder) : Category;
    public string Name => Path.GetFileName(SourcePath);
    public string Destination => Path.GetRelativePath(Path.GetDirectoryName(SourcePath)!, DestinationPath);
}
