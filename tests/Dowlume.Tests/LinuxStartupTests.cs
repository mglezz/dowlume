using Dowlume.App.Services;

namespace Dowlume.Tests;

public sealed class LinuxStartupTests
{
    [Theory]
    [InlineData("/opt/Dowlume App/Dowlume.App", "\"/opt/Dowlume App/Dowlume.App\"")]
    [InlineData("/opt/$HOME/Dowlume.App", "\"/opt/\\\\$HOME/Dowlume.App\"")]
    [InlineData("/opt/`echo hi`/Dowlume.App", "\"/opt/\\\\`echo hi\\\\`/Dowlume.App\"")]
    public void ExecEscapesDesktopEntryAndArgumentLayers(string path, string expected) =>
        Assert.Equal(expected, LinuxStartupRegistration.QuoteExecutable(path));

    [Theory]
    [InlineData("/opt/%f/Dowlume.App")]
    [InlineData("/opt/line\nbreak/Dowlume.App")]
    [InlineData("/opt/a=b/Dowlume.App")]
    public void ExecRejectsFieldCodesAndUnrepresentablePaths(string path) =>
        Assert.Throws<ArgumentException>(() => LinuxStartupRegistration.QuoteExecutable(path));

    [LinuxFact]
    public void StartupIsOptInPersistsAndHonorsDesktopDisable()
    {
        using var workspace = new TemporaryWorkspace();
        string executable = workspace.CreateFile("Dowlume.App");
        var startup = new LinuxStartupRegistration(executable, workspace.Root);
        Assert.True(startup.IsSupported);
        Assert.False(startup.IsEnabled);
        startup.SetEnabled(true);
        Assert.True(new LinuxStartupRegistration(executable, workspace.Root).IsEnabled);
        string desktop = Path.Combine(workspace.Root, "autostart", "com.mglezz.dowlume.desktop");
        File.AppendAllText(desktop, "Hidden=true\n");
        Assert.False(startup.IsEnabled);
        startup.SetEnabled(true);
        Assert.True(startup.IsEnabled);
        startup.SetEnabled(false);
        Assert.False(File.Exists(desktop));
    }

    [LinuxFact]
    public void StartupPreservesUnrelatedEntriesAndRefusesSymlinks()
    {
        using var workspace = new TemporaryWorkspace();
        var startup = new LinuxStartupRegistration(workspace.CreateFile("Dowlume.App"), workspace.Root);
        string folder = Path.Combine(workspace.Root, "autostart");
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, "com.mglezz.dowlume.desktop");
        File.WriteAllText(path, "[Desktop Entry]\nType=Application\nExec=other\n");
        Assert.Throws<IOException>(() => startup.SetEnabled(true));
        Assert.Throws<IOException>(() => startup.SetEnabled(false));
        Assert.Contains("Exec=other", File.ReadAllText(path));
        File.Delete(path);
        string target = workspace.CreateFile("unrelated", "preserve");
        File.CreateSymbolicLink(path, target);
        Assert.Throws<IOException>(() => startup.SetEnabled(true));
        Assert.Equal("preserve", File.ReadAllText(target));
    }
}
