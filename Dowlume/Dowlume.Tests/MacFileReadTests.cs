using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Dowlume.Infrastructure;

namespace Dowlume.Tests;

[SupportedOSPlatform("macos")]
[Trait("Category", "MacNativeRead")]
public sealed class MacFileReadTests
{
    [MacFact]
    public void ProductionHashMatchesContentAndNativeMetadataMatchesSystemStat()
    {
        using var workspace = new TemporaryWorkspace();
        string path = workspace.CreateFile("espacio café 🗂.txt", "Contenido de prueba");
        File.SetLastWriteTimeUtc(path, new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc));
        using FileStream stream = MacFileHandle.OpenRead(path);
        MacFileHandle.Snapshot snapshot = MacFileHandle.Capture(stream.SafeFileHandle);
        string[] system = Run("/usr/bin/stat", "-f", "%d:%i:%l:%p:%z:%m:%c:%B:%f", path).Trim().Split(':');
        Assert.Equal(int.Parse(system[0], CultureInfo.InvariantCulture), snapshot.Device);
        Assert.Equal(ulong.Parse(system[1], CultureInfo.InvariantCulture), snapshot.Inode);
        Assert.Equal(ushort.Parse(system[2], CultureInfo.InvariantCulture), snapshot.LinkCount);
        Assert.Equal(Convert.ToUInt16(system[3], 8), snapshot.Mode);
        Assert.Equal(long.Parse(system[4], CultureInfo.InvariantCulture), snapshot.Size);
        Assert.Equal(long.Parse(system[5], CultureInfo.InvariantCulture), snapshot.ModifiedSeconds);
        Assert.Equal(long.Parse(system[6], CultureInfo.InvariantCulture), snapshot.ChangedSeconds);
        Assert.Equal(long.Parse(system[7], CultureInfo.InvariantCulture), snapshot.BirthSeconds);
        Assert.Equal(uint.Parse(system[8], CultureInfo.InvariantCulture), snapshot.Flags);
        string expected = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("Contenido de prueba")));
        Assert.Equal(expected, new SafeFileMover().Hash(path));
        MacFileHandle.VerifyUnchanged(path, stream.SafeFileHandle, snapshot);
        Assert.False(new SafeFileMover().CanMove);
    }

    [MacFact]
    public void KernelOpenRejectsFinalAndAncestorSymbolicLinks()
    {
        using var workspace = new TemporaryWorkspace();
        string path = workspace.CreateFile("real/sample.txt");
        string fileLink = Path.Combine(workspace.Downloads, "linked.txt");
        string directoryLink = Path.Combine(workspace.Downloads, "linked-directory");
        File.CreateSymbolicLink(fileLink, path);
        Directory.CreateSymbolicLink(directoryLink, Path.GetDirectoryName(path)!);
        Assert.Throws<IOException>(() => MacFileHandle.OpenRead(fileLink));
        Assert.Throws<IOException>(() => MacFileHandle.OpenRead(Path.Combine(directoryLink, "sample.txt")));
        Assert.Throws<IOException>(() => new SafeFileMover().Hash(fileLink));
        Assert.Equal("Disposable test data", File.ReadAllText(path));
    }

    [MacFact]
    public void NonRegularFilesAreRejectedWithoutWaitingForFifoWriters()
    {
        using var workspace = new TemporaryWorkspace();
        string fifo = Path.Combine(workspace.Downloads, "pipe");
        Run("/usr/bin/mkfifo", fifo);
        Assert.Throws<IOException>(() => MacFileHandle.OpenRead(fifo));
        Assert.Throws<IOException>(() => MacFileHandle.OpenRead(workspace.Downloads));
    }

    [MacFact]
    public void ReplacementWithIdenticalBytesDoesNotPassIdentityVerification()
    {
        using var workspace = new TemporaryWorkspace();
        string path = workspace.CreateFile("sample.txt");
        using FileStream stream = MacFileHandle.OpenRead(path);
        MacFileHandle.Snapshot before = MacFileHandle.Capture(stream.SafeFileHandle);
        File.Move(path, path + ".original");
        workspace.CreateFile("sample.txt");
        File.SetLastWriteTimeUtc(path, File.GetLastWriteTimeUtc(path + ".original"));
        Assert.Throws<IOException>(() => MacFileHandle.VerifyUnchanged(path, stream.SafeFileHandle, before));
        Assert.Equal(File.ReadAllText(path + ".original"), File.ReadAllText(path));
    }

    [MacFact]
    public void AncestorReplacementDoesNotRedirectTheOpenedFile()
    {
        using var workspace = new TemporaryWorkspace();
        string path = workspace.CreateFile("folder/sample.txt", "original");
        using FileStream stream = MacFileHandle.OpenRead(path);
        MacFileHandle.Snapshot before = MacFileHandle.Capture(stream.SafeFileHandle);
        Directory.Move(Path.GetDirectoryName(path)!, Path.Combine(workspace.Downloads, "original-folder"));
        workspace.CreateFile("folder/sample.txt", "replacement");
        using var reader = new StreamReader(stream, leaveOpen: true);
        Assert.Equal("original", reader.ReadToEnd());
        Assert.Throws<IOException>(() => MacFileHandle.VerifyUnchanged(path, stream.SafeFileHandle, before));
        Assert.Equal("replacement", File.ReadAllText(path));
    }

    [MacFact]
    public void ConcurrentWriteIsDetectedEvenWhenLengthAndModificationTimeAreRestored()
    {
        using var workspace = new TemporaryWorkspace();
        string path = workspace.CreateFile("sample.txt", "before");
        DateTime modified = File.GetLastWriteTimeUtc(path);
        using FileStream stream = MacFileHandle.OpenRead(path);
        MacFileHandle.Snapshot before = MacFileHandle.Capture(stream.SafeFileHandle);
        // POSIX writers are not excluded by the read descriptor. This must remain read-only.
        File.WriteAllText(path, "after!");
        File.SetLastWriteTimeUtc(path, modified);
        Assert.Throws<IOException>(() => MacFileHandle.VerifyUnchanged(path, stream.SafeFileHandle, before));
        Assert.Throws<IOException>(() => new SafeFileMover().OpenVerifiedRead(path,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("before")))));
        Assert.Equal("after!", File.ReadAllText(path));
    }

    [MacFact]
    public void CaseAndUnicodeAliasesFollowActualVolumeIdentity()
    {
        using var workspace = new TemporaryWorkspace();
        string path = workspace.CreateFile("café.txt");
        using FileStream stream = MacFileHandle.OpenRead(path);
        bool sensitive = MacFileHandle.IsCaseSensitive(stream.SafeFileHandle);
        string? expectedPolicy = Environment.GetEnvironmentVariable("DOWLUME_TEST_CASE_SENSITIVE");
        if (expectedPolicy is not null)
        {
            Assert.Equal(bool.Parse(expectedPolicy), sensitive);
        }
        string uppercase = Path.Combine(workspace.Downloads, "CAFÉ.TXT");
        Assert.Equal(!sensitive, File.Exists(uppercase));
        if (sensitive)
        {
            workspace.CreateFile("CAFÉ.TXT", "distinct file");
        }
        using FileStream caseAlias = MacFileHandle.OpenRead(uppercase);
        Assert.Equal(!sensitive, MacFileHandle.Capture(stream.SafeFileHandle).Inode == MacFileHandle.Capture(caseAlias.SafeFileHandle).Inode);
        using FileStream unicodeAlias = MacFileHandle.OpenRead(Path.Combine(workspace.Downloads, "cafe\u0301.txt"));
        Assert.Equal(MacFileHandle.Capture(stream.SafeFileHandle), MacFileHandle.Capture(unicodeAlias.SafeFileHandle));
    }

    [MacFact]
    public void HardLinksExposeSharedIdentityAndLinkCount()
    {
        using var workspace = new TemporaryWorkspace();
        string path = workspace.CreateFile("sample.txt");
        string alias = Path.Combine(workspace.Downloads, "alias.txt");
        Run("/bin/ln", path, alias);
        using FileStream first = MacFileHandle.OpenRead(path);
        using FileStream second = MacFileHandle.OpenRead(alias);
        MacFileHandle.Snapshot snapshot = MacFileHandle.Capture(first.SafeFileHandle);
        Assert.Equal(2, snapshot.LinkCount);
        Assert.Equal(snapshot, MacFileHandle.Capture(second.SafeFileHandle));
    }

    [MacFact]
    public void PermissionDenialDoesNotChangeContentOrPermissions()
    {
        using var workspace = new TemporaryWorkspace();
        string path = workspace.CreateFile("sample.txt");
        UnixFileMode original = File.GetUnixFileMode(path);
        try
        {
            File.SetUnixFileMode(path, UnixFileMode.None);
            Assert.Throws<UnauthorizedAccessException>(() => new SafeFileMover().Hash(path));
            Assert.Equal(UnixFileMode.None, File.GetUnixFileMode(path));
        }
        finally
        {
            File.SetUnixFileMode(path, original);
        }
        Assert.Equal("Disposable test data", File.ReadAllText(path));
    }

    [MacFact]
    public void InvalidPathsCannotBeTruncatedByNativeMarshalling()
    {
        using var workspace = new TemporaryWorkspace();
        string path = workspace.CreateFile("sample.txt");
        Assert.Throws<ArgumentException>(() => MacFileHandle.OpenRead(path + "\0ignored"));
        Assert.Throws<ArgumentException>(() => MacFileHandle.OpenRead("relative.txt"));
        Assert.Throws<IOException>(() => MacFileHandle.OpenRead(path + ".missing"));
    }

    private static string Run(string executable, params string[] arguments)
    {
        var start = new ProcessStartInfo(executable) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (string argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }
        using Process process = Process.Start(start)!;
        Assert.True(process.WaitForExit(10000), "The native fixture command timed out.");
        Assert.True(process.ExitCode == 0, process.StandardError.ReadToEnd());
        return process.StandardOutput.ReadToEnd();
    }
}
