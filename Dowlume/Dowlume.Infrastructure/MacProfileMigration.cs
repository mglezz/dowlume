namespace Dowlume.Infrastructure;

public static class MacProfileMigration
{
    public static void ImportPreview(string preview, string destination)
    {
        if (File.Exists(Path.Combine(destination, "dowlume.db")) || !File.Exists(Path.Combine(preview, "dowlume.db")))
            return;
        PathSafety.RejectLinks(preview);
        PathSafety.RejectLinks(destination);
        if (Directory.Exists(destination))
            throw new IOException("The new profile folder already exists without a database. Back it up before migration.");
        string lockPath = Path.Combine(preview, "instance.lock");
        PathSafety.RejectLinks(lockPath);
        using var sourceLock = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        string staging = destination + ".migration-" + Guid.NewGuid().ToString("N");
        try
        {
            CopyDirectory(preview, staging);
            // Verify the copied database before making the complete profile visible.
            var store = new SqliteStateStore(Path.Combine(staging, "dowlume.db"));
            _ = store.LoadSettings();
            _ = store.GetFiles();
            _ = store.GetOperations();
            Directory.Move(staging, destination);
        }
        finally { if (Directory.Exists(staging)) Directory.Delete(staging, recursive: true); }
    }

    private static void CopyDirectory(string source, string target)
    {
        PathSafety.RejectLinks(source);
        Directory.CreateDirectory(target);
        foreach (string file in Directory.EnumerateFiles(source))
        {
            if (Path.GetFileName(file) == "instance.lock")
                continue;
            PathSafety.RejectLinks(file);
            File.Copy(file, Path.Combine(target, Path.GetFileName(file)), overwrite: false);
        }
        foreach (string directory in Directory.EnumerateDirectories(source))
            CopyDirectory(directory, Path.Combine(target, Path.GetFileName(directory)));
    }
}
