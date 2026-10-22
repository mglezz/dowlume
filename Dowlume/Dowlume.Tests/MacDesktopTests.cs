using System.Runtime.Versioning;
using System.Xml.Linq;
using Dowlume.App.Services;
using Dowlume.Core;
using Dowlume.Infrastructure;

namespace Dowlume.Tests;

[SupportedOSPlatform("macos")]
public sealed class MacDesktopTests
{
    [MacFact]
    public void LoginConfigurationUsesLiteralPathsAndOnlyRemovesItsOwnJob()
    {
        using var workspace = new TemporaryWorkspace();
        string executable = workspace.CreateFile("Apps & spaces/Dowlume.app/Contents/MacOS/Dowlume.App");
        string jobs = Path.Combine(workspace.Root, "LaunchAgents");
        var startup = new MacStartupRegistration(executable, jobs);
        Assert.True(startup.IsSupported);
        Assert.False(startup.IsEnabled);
        startup.SetEnabled(true);
        string job = Assert.Single(Directory.EnumerateFiles(jobs));
        Assert.Contains(executable, XDocument.Load(job).Descendants("string").Select(node => node.Value));
        Assert.True(startup.IsEnabled);
        startup.SetEnabled(false);
        Assert.Empty(Directory.EnumerateFiles(jobs));
        File.WriteAllText(job, "<plist><dict><key>Label</key><string>unrelated</string></dict></plist>");
        string unrelated = File.ReadAllText(job);
        Assert.Throws<IOException>(() => startup.SetEnabled(true));
        Assert.Throws<IOException>(() => startup.SetEnabled(false));
        Assert.Equal(unrelated, File.ReadAllText(job));
    }

    [MacFact]
    public void PreviewProfileMigrationRetainsSettingsHistoryAndOriginal()
    {
        using var workspace = new TemporaryWorkspace();
        string preview = Path.Combine(workspace.Root, "Dowlume.Preview");
        string target = Path.Combine(workspace.Root, "Dowlume");
        var original = new SqliteStateStore(Path.Combine(preview, "dowlume.db"));
        original.SaveSettings(new AppSettings { Language = "es", DownloadsPath = workspace.Downloads });
        string sample = workspace.CreateFile("sample.txt");
        var file = new TrackedFile(Guid.NewGuid(), sample, sample, FileCategory.Documents, DateTimeOffset.UtcNow, LifecycleStatus.Kept, new SafeFileMover().Hash(sample));
        original.SaveFile(file);
        MacProfileMigration.ImportPreview(preview, target);
        var migrated = new SqliteStateStore(Path.Combine(target, "dowlume.db"));
        Assert.Equal("es", migrated.LoadSettings()!.Language);
        Assert.Equal(file, Assert.Single(migrated.GetFiles()));
        Assert.Equal(file, Assert.Single(original.GetFiles()));
        migrated.SaveSettings(migrated.LoadSettings()! with
        {
            Language = "en"
        });
        MacProfileMigration.ImportPreview(preview, target);
        Assert.Equal("en", migrated.LoadSettings()!.Language);
        Assert.Equal("es", original.LoadSettings()!.Language);
    }

    [MacFact]
    public void MigrationRefusesLiveProfilesAndLinksWithoutPartialDestination()
    {
        using var workspace = new TemporaryWorkspace();
        string preview = Path.Combine(workspace.Root, "Preview");
        string target = Path.Combine(workspace.Root, "Stable");
        _ = new SqliteStateStore(Path.Combine(preview, "dowlume.db"));
        using (var locked = new FileStream(Path.Combine(preview, "instance.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            Assert.Throws<IOException>(() => MacProfileMigration.ImportPreview(preview, target));
        Assert.False(Directory.Exists(target));
        File.CreateSymbolicLink(Path.Combine(preview, "unexpected-link"), workspace.CreateFile("outside.txt"));
        Assert.Throws<IOException>(() => MacProfileMigration.ImportPreview(preview, target));
        Assert.False(Directory.Exists(target));
        Assert.Empty(Directory.EnumerateDirectories(workspace.Root, "*.migration-*"));
    }
}
