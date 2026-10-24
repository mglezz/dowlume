using Dowlume.Core;
using Dowlume.Infrastructure;

namespace Dowlume.Tests;

public sealed class SafetyTests
{
    [Fact]
    public void SiblingWithMatchingPrefixIsOutsideManagedFolder()
    {
        using var workspace = new TemporaryWorkspace();
        Assert.False(PathSafety.IsWithin(workspace.Downloads, workspace.Downloads + "-other/file.txt"));
        Assert.Throws<IOException>(() => PathSafety.RequireWithin(workspace.Downloads, Path.Combine(workspace.Downloads, "..", "outside.txt")));
    }

    [Fact]
    public void LockedSourceAndExistingDestinationArePreserved()
    {
        using var workspace = new TemporaryWorkspace();
        string source = workspace.CreateFile("a.txt", "Source");
        string destination = workspace.CreateFile("b.txt", "Destination");
        var mover = FixtureFileMover.Create(workspace);
        string hash = mover.Hash(source);
        using (var locked = new FileStream(source, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            Assert.Throws<IOException>(() => mover.Move(source, destination, hash));
        }
        Assert.Throws<IOException>(() => mover.Move(source, destination, hash));
        Assert.Equal("Source", File.ReadAllText(source));
        Assert.Equal("Destination", File.ReadAllText(destination));
    }

    [Fact]
    public void SourceSurvivesAnInvalidDestination()
    {
        using var workspace = new TemporaryWorkspace();
        string source = workspace.CreateFile("source.txt");
        string blockingFile = workspace.CreateFile("not-a-folder");
        var mover = FixtureFileMover.Create(workspace);
        Assert.ThrowsAny<IOException>(() => mover.Move(source, Path.Combine(blockingFile, "file.txt"), mover.Hash(source)));
        Assert.True(File.Exists(source));
    }

    [Fact]
    public void DiagnosticsDoNotIncludePersonalPathsOrExceptionMessages()
    {
        using var workspace = new TemporaryWorkspace();
        string path = Path.Combine(workspace.Root, "log.jsonl");
        new DiagnosticLog(path).Write("test_failure", new IOException("secret-user/private-file.pdf"));
        string log = File.ReadAllText(path);
        Assert.DoesNotContain("secret-user", log);
        Assert.Contains("IOException", log);
    }

}
