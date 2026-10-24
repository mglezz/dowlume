using Dowlume.Infrastructure;

namespace Dowlume.Tests;

public sealed class XdgUserDirectoriesTests
{
    [Fact]
    public void ReadsConfiguredDownloadsAsDataWithoutExecutingShellSyntax()
    {
        using var workspace = new TemporaryWorkspace();
        string home = workspace.Root;
        string expected = Path.Combine(home, "Mis descargas");
        Assert.Equal(expected, XdgUserDirectories.ParseDownloads("# Preferences\nXDG_DOWNLOAD_DIR=\"$HOME/Mis descargas\" # localized", home));
        Assert.Equal(home, XdgUserDirectories.ParseDownloads("XDG_DOWNLOAD_DIR=\"$HOME\"", home));
        Assert.Null(XdgUserDirectories.ParseDownloads("XDG_DOWNLOAD_DIR=\"$(touch injected)\"", home));
        Assert.Null(XdgUserDirectories.ParseDownloads("XDG_DOWNLOAD_DIR=\"`touch injected`\"", home));
        Assert.Null(XdgUserDirectories.ParseDownloads("XDG_DOWNLOAD_DIR=\"$OTHER/downloads\"", home));
        Assert.Null(XdgUserDirectories.ParseDownloads("XDG_DOWNLOAD_DIR=\"relative/folder\"", home));
        Assert.Null(XdgUserDirectories.ParseDownloads("XDG_DOWNLOAD_DIR=\"$HOME/path\"; touch injected", home));
        Assert.False(File.Exists(Path.Combine(home, "injected")));
    }

    [Fact]
    public void ReadsEscapedLiteralCharactersAndRejectsUnboundedInput()
    {
        using var workspace = new TemporaryWorkspace();
        Assert.Equal(Path.Combine(workspace.Root, "$literal"),
            XdgUserDirectories.ParseDownloads("XDG_DOWNLOAD_DIR=\"$HOME/\\$literal\"", workspace.Root));
        Assert.Null(XdgUserDirectories.ParseDownloads(new string('x', 65537), workspace.Root));
        Assert.Null(XdgUserDirectories.ParseDownloads("XDG_DOWNLOAD_DIR=\"$HOME/Downloads\"", "relative-home"));
    }
}
