using Dowlume.Core;
using Dowlume.Infrastructure;

namespace Dowlume.Tests;

public sealed class CustomRuleTests
{
    private static CategoryRule Custom(string folder, RuleMatchKind kind, string pattern) =>
        new(FileCategory.Other, folder, kind == RuleMatchKind.Extension ? [pattern] : [], 1)
        {
            Id = Guid.NewGuid(),
            MatchKind = kind,
            NamePattern = kind == RuleMatchKind.Extension ? string.Empty : pattern
        };

    [Fact]
    public void NamesOverrideExtensionsWithoutDependingOnRuleOrder()
    {
        var settings = new AppSettings { DownloadsPath = Path.GetTempPath() };
        CategoryRule contracts = Custom("Contracts", RuleMatchKind.NameContains, "contrato");
        CategoryRule projects = Custom("Projects", RuleMatchKind.NameStartsWith, "ABC");
        settings.Categories.AddRange([contracts, projects]);
        settings.Validate();
        Assert.Equal(contracts.Id, FileClassifier.Classify("Mi CONTRATO.pdf", settings).Id);
        Assert.Equal(projects.Id, FileClassifier.Classify("abc-123.MP4", settings).Id);
        Assert.Equal(FileCategory.Documents, FileClassifier.Classify("report.pdf", settings).Category);
        Assert.Null(FileClassifier.Classify("report.contrato", settings).Id);
        settings.Categories.Reverse();
        Assert.Equal(contracts.Id, FileClassifier.Classify("Mi CONTRATO.pdf", settings).Id);
        Assert.Throws<RuleConflictException>(() => FileClassifier.Classify("ABC contrato.pdf", settings));
        Assert.True(FileClassifier.IsExcluded("ABC contrato.crdownload", settings));
    }

    [Fact]
    public void DuplicateExtensionsNamesFoldersAndInvalidMatchersAreRejected()
    {
        var settings = new AppSettings { DownloadsPath = Path.GetTempPath() };
        CategoryRule rule = Custom("Photos", RuleMatchKind.Extension, ".PNG");
        settings.Categories.Add(rule);
        Assert.Throws<RuleConflictException>(settings.Validate);
        settings.Categories[^1] = rule with
        {
            Extensions = [".custom"]
        };
        settings.Validate();
        settings.Categories.Add(Custom("PHOTOS", RuleMatchKind.NameContains, "contract"));
        Assert.Throws<ArgumentException>(settings.Validate);
        settings.Categories.RemoveAt(settings.Categories.Count - 1);
        settings.Categories.Add(Custom("Contracts", RuleMatchKind.NameContains, "contract"));
        settings.Categories.Add(Custom("Projects", RuleMatchKind.NameStartsWith, "CONTRACT"));
        Assert.Throws<RuleConflictException>(settings.Validate);
        settings.Categories.RemoveAt(settings.Categories.Count - 1);
        foreach (string bad in new[] { "", "*contract", "../contract", "contract?", "\n" })
        {
            settings.Categories[^1] = settings.Categories[^1] with
            {
                NamePattern = bad
            };
            Assert.Throws<ArgumentException>(settings.Validate);
        }
    }

    [Fact]
    public void RuleEditsInvalidateConsentAndUnicodeNamesMatch()
    {
        var original = new AppSettings { DownloadsPath = Path.GetTempPath() };
        CategoryRule rule = Custom("Cafés", RuleMatchKind.NameContains, "café");
        original.Categories.Add(rule);
        var copy = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(System.Text.Json.JsonSerializer.Serialize(original))!;
        Assert.True(original.HasSameValuesAs(copy));
        Assert.Equal(rule.Id, FileClassifier.Classify("Cafe\u0301.pdf", copy).Id);
        copy.Categories[^1] = copy.Categories[^1] with
        {
            MatchKind = RuleMatchKind.NameStartsWith
        };
        Assert.False(original.HasSameValuesAs(copy));
        copy.Categories[^1] = rule with
        {
            NamePattern = "different"
        };
        Assert.False(original.HasSameValuesAs(copy));
        copy.Categories[^1] = rule with
        {
            Id = Guid.NewGuid()
        };
        Assert.False(original.HasSameValuesAs(copy));
    }

    [Fact]
    public async Task AmbiguousFilesNeverMoveAndPartialDownloadsStayExcluded()
    {
        using var workspace = new TemporaryWorkspace();
        var settings = new AppSettings { DownloadsPath = workspace.Downloads };
        settings.Categories.AddRange([Custom("Contracts", RuleMatchKind.NameContains, "contract"), Custom("Projects", RuleMatchKind.NameStartsWith, "ABC")]);
        var store = new SqliteStateStore(workspace.Database);
        var organizer = new DownloadOrganizer(store, FixtureFileMover.Create(workspace), new Ready());
        string source = workspace.CreateFile("ABC contract.pdf", "preserve me");
        await Assert.ThrowsAsync<RuleConflictException>(() => organizer.PreviewAsync(settings));
        await Assert.ThrowsAsync<RuleConflictException>(() => organizer.OrganizeAsync(source, settings));
        Assert.Equal("preserve me", File.ReadAllText(source));
        Assert.Empty(store.GetOperations());
        Assert.Empty(Directory.GetDirectories(workspace.Downloads));
        string partial = workspace.CreateFile("contract.crdownload");
        await organizer.OrganizeAsync(partial, settings);
        Assert.True(File.Exists(partial));
    }

    [Fact]
    public async Task RemovedRulesRetainReviewKeepSnoozeAndUndoAcrossDatabaseReopen()
    {
        using var workspace = new TemporaryWorkspace();
        var settings = new AppSettings { DownloadsPath = workspace.Downloads };
        CategoryRule rule = Custom("Contracts", RuleMatchKind.NameContains, "contract");
        settings.Categories.Add(rule);
        var store = new SqliteStateStore(workspace.Database);
        store.SaveSettings(settings);
        var clock = new Clock();
        var mover = FixtureFileMover.Create(workspace);
        var organizer = new DownloadOrganizer(store, mover, new Ready(), clock);
        string source = workspace.CreateFile("contract.pdf", "retained content");
        PreviewItem preview = Assert.Single(await organizer.PreviewAsync(settings));
        Assert.Equal("Contracts", preview.CustomFolder);
        await organizer.OrganizeAsync(source, settings, expectedHash: preview.ContentHash);
        var reopened = new SqliteStateStore(workspace.Database);
        var resumed = new DownloadOrganizer(reopened, mover, new Ready(), clock);
        TrackedFile file = Assert.Single(reopened.GetFiles());
        Assert.Equal(rule.Id, file.CustomRuleId);
        Assert.Equal("Contracts", file.AssignedFolder);
        settings.Categories.Remove(rule);
        reopened.SaveSettings(settings);
        clock.Now = clock.Now.AddDays(2);
        await resumed.ReconcileAsync(settings);
        Assert.Equal(LifecycleStatus.InReview, Assert.Single(reopened.GetFiles()).Status);
        await resumed.SnoozeAsync(file.Id, settings);
        Assert.Equal(Path.Combine(workspace.Downloads, "Contracts", "contract.pdf"), Assert.Single(reopened.GetFiles()).CurrentPath);
        clock.Now = clock.Now.AddDays(8);
        await resumed.ReconcileAsync(settings);
        await resumed.KeepAsync(file.Id, settings);
        OperationRecord keep = reopened.GetOperations().Last();
        // Separate deserializations must preserve value equality for Undo.
        await resumed.UndoAsync(keep.Id, settings);
        Assert.Equal(LifecycleStatus.InReview, Assert.Single(reopened.GetFiles()).Status);
        Assert.Equal("retained content", File.ReadAllText(Assert.Single(reopened.GetFiles()).CurrentPath));
    }

    private sealed class Ready : IFileReadiness
    {
        public Task<bool> IsReadyAsync(string path, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
