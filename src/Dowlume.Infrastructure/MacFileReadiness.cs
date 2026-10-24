using System.Runtime.Versioning;
using Dowlume.Core;

namespace Dowlume.Infrastructure;

[SupportedOSPlatform("macos")]
public sealed class MacFileReadiness(TimeSpan? minimumAge = null) : IFileReadiness
{
    public Task<bool> IsReadyAsync(string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            using FileStream stream = MacFileHandle.OpenRead(path);
            MacFileHandle.Snapshot before = MacFileHandle.Capture(stream.SafeFileHandle);
            DateTimeOffset modified = DateTimeOffset.FromUnixTimeSeconds(before.ModifiedSeconds).AddTicks(before.ModifiedNanoseconds / 100);
            bool ready = DateTimeOffset.UtcNow - modified >= (minimumAge ?? TimeSpan.FromSeconds(3));
            // Readiness is a discovery heuristic. The prepared move separately verifies content and identity.
            MacFileHandle.VerifyUnchanged(path, stream.SafeFileHandle, before);
            return Task.FromResult(ready);
        }
        catch (IOException) { return Task.FromResult(false); }
        catch (UnauthorizedAccessException) { return Task.FromResult(false); }
    }
}
