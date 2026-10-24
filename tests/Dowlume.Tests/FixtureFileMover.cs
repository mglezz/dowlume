using Dowlume.Core;
using Dowlume.Infrastructure;

namespace Dowlume.Tests;

// Exercises shared orchestration with disposable files, not native race protection.
// This adapter is compiled only into the test assembly and has no application opt-in.
internal sealed class FixtureFileMover(TemporaryWorkspace workspace) : IFileMover
{
    private readonly SafeFileMover _reader = new();
    public bool CanMove => true;

    public static IFileMover Create(TemporaryWorkspace workspace) =>
        OperatingSystem.IsWindows() ? new SafeFileMover() : new FixtureFileMover(workspace);

    public string Hash(string path)
    {
        PathSafety.RequireWithin(workspace.Root, path);
        return _reader.Hash(path);
    }

    public void Move(string source, string destination, string expectedHash)
    {
        PathSafety.RequireWithin(workspace.Root, source);
        PathSafety.RequireWithin(workspace.Root, destination);
        using FileStream stream = _reader.OpenVerifiedRead(source, expectedHash);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        PathSafety.RejectLinks(destination);
        File.Move(source, destination, overwrite: false);
        Assert.Equal(expectedHash, Hash(destination));
    }
}
