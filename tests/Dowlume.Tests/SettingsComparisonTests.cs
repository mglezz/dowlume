using Dowlume.Core;

namespace Dowlume.Tests;

public sealed class SettingsComparisonTests
{
    [Fact]
    public void IndependentSettingsWithTheSameValuesMatch()
    {
        var preview = new AppSettings { DownloadsPath = "downloads", ExcludedExtensions = [".log", ".bak"] };
        var current = new AppSettings { DownloadsPath = "downloads", ExcludedExtensions = [".log", ".bak"] };

        Assert.NotSame(preview.Categories, current.Categories);
        Assert.NotSame(preview.Categories[0].Extensions, current.Categories[0].Extensions);
        Assert.True(current.HasSameValuesAs(preview));
        Assert.False(current.HasSameValuesAs(null));
    }

    [Theory]
    [InlineData("language")]
    [InlineData("path")]
    [InlineData("exclusions")]
    [InlineData("exclusion order")]
    [InlineData("category order")]
    [InlineData("category count")]
    [InlineData("category")]
    [InlineData("folder")]
    [InlineData("review period")]
    [InlineData("extension case")]
    [InlineData("extension order")]
    public void ChangedSettingsRequireANewPreview(string change)
    {
        var preview = new AppSettings { DownloadsPath = "downloads", ExcludedExtensions = [".log", ".bak"] };
        var current = new AppSettings { DownloadsPath = "downloads", ExcludedExtensions = [".log", ".bak"] };
        switch (change)
        {
            case "language":
                current = current with
                {
                    Language = "es"
                };
                break;
            case "path":
                current = current with
                {
                    DownloadsPath = "Downloads"
                };
                break;
            case "exclusions":
                current.ExcludedExtensions[0] = ".tmp";
                break;
            case "exclusion order":
                Array.Reverse(current.ExcludedExtensions);
                break;
            case "category order":
                current.Categories.Reverse();
                break;
            case "category count":
                current.Categories.RemoveAt(0);
                break;
            case "category":
                current.Categories[0] = current.Categories[0] with
                {
                    Category = FileCategory.Other
                };
                break;
            case "folder":
                current.Categories[0] = current.Categories[0] with
                {
                    FolderName = "New folder"
                };
                break;
            case "review period":
                current.Categories[0] = current.Categories[0] with
                {
                    ReviewAfterDays = 31
                };
                break;
            case "extension case":
                current.Categories[0].Extensions[0] = ".PDF";
                break;
            case "extension order":
                Array.Reverse(current.Categories[0].Extensions);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(change));
        }

        Assert.False(current.HasSameValuesAs(preview));
        Assert.False(preview.HasSameValuesAs(current));
    }
}
