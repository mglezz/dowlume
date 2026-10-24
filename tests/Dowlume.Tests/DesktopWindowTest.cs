using Avalonia.Controls;

namespace Dowlume.Tests;

internal static class DesktopWindowTest
{
    internal static async Task CloseAsync(Window window)
    {
        // Close requests may return while MainWindow drains refreshes and database work.
        // The disposable profile must outlive the actual Closed event, especially on Windows.
        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnClosed(object? sender, EventArgs args) => closed.TrySetResult();
        window.Closed += OnClosed;
        try
        {
            window.Close();
            await closed.Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally { window.Closed -= OnClosed; }
    }
}
