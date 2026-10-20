namespace Dowlume.Core;

/// <summary>Verified file mutation supplied by an accepted native implementation.</summary>
public interface IFileMover
{
    bool CanMove
    {
        get;
    }
    string Hash(string path);
    void Move(string source, string destination, string expectedHash);
}
