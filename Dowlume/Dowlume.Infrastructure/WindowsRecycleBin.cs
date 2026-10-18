using Dowlume.Core;

namespace Dowlume.Infrastructure;

// Compatibility entry point only. Native shell deletion is deliberately absent from v0.1.0.
public sealed class WindowsRecycleBin : IRecycleBin
{
    public const string DisabledMessage = "Recycling is disabled in v0.1.0 because Windows Recycle Bin could not reliably preserve files. Use Keep, Snooze, or Move to instead.";

    public Task RecycleAsync(string path, string expectedHash) =>
        Task.FromException(new NotSupportedException(DisabledMessage));
}
