namespace Dowlume.Core;

// Persistent identity, independent of the pathname and the current mount's device number.
public sealed record FileIdentity(string Volume, ulong FileId, long BirthSeconds, long BirthNanoseconds);

public interface IPreparedFileMove : IDisposable
{
    FileIdentity? SourceIdentity
    {
        get;
    }
    string? ObservedPath
    {
        get;
    }
    void Execute();
}

public interface IPreparedFileMover
{
    IPreparedFileMove PrepareMove(string source, string destination, string expectedHash);
    FileIdentity? GetIdentity(string path);
    bool MatchesRecoveryFile(string path, FileIdentity identity, string expectedHash);
}

public sealed class DeferredFileMove(Action execute) : IPreparedFileMove
{
    public FileIdentity? SourceIdentity => null;
    public string? ObservedPath => null;
    public void Execute() => execute();
    public void Dispose()
    {
    }
}
