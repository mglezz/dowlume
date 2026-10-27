using System.Runtime.Versioning;
using Dowlume.Core;

namespace Dowlume.Infrastructure;

[SupportedOSPlatform("linux")]
public sealed class LinuxFileReadiness(TimeSpan? minimumAge = null) : IFileReadiness
{
    public Task<bool> IsReadyAsync(string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            using FileStream stream = LinuxFileSystem.OpenRead(path);
            LinuxFileSystem.Snapshot before = LinuxFileSystem.Capture(stream.SafeFileHandle);
            DateTimeOffset modified = DateTimeOffset.FromUnixTimeSeconds(before.ModifiedSeconds).AddTicks(before.ModifiedNanoseconds / 100);
            bool ready = DateTimeOffset.UtcNow - modified >= (minimumAge ?? TimeSpan.FromSeconds(3));
            // Readiness is a discovery heuristic. The prepared move separately verifies content and identity.
            LinuxFileSystem.VerifyUnchanged(path, stream.SafeFileHandle, before);
            return Task.FromResult(ready);
        }
        catch (NotSupportedException) { return Task.FromResult(false); }
        catch (IOException) { return Task.FromResult(false); }
        catch (UnauthorizedAccessException) { return Task.FromResult(false); }
    }
}
