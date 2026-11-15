using System.Collections.Concurrent;
using Avalonia.Controls;
using Avalonia.Headless;
using Dowlume.App.Services;
using Dowlume.App.Views;

namespace Dowlume.Tests;

[Collection("Desktop")]
public sealed class InstanceActivationTests
{
    [Fact]
    public async Task ActivationIsProfileScopedAndListenerSurvivesRepeatedLaunches()
    {
        using var workspace = new TemporaryWorkspace();
        int activations = 0;
        var errors = new ConcurrentQueue<Exception>();
        using (var owner = new InstanceActivation(workspace.Root, () => Interlocked.Increment(ref activations), errors.Enqueue))
        {
            for (int launch = 0; launch < 3; launch++)
                Assert.True(await InstanceActivation.TryActivateAsync(workspace.Root));
            Assert.Equal(3, activations);
            Assert.All(await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => InstanceActivation.TryActivateAsync(workspace.Root))), Assert.True);
            Assert.Equal(6, activations);
            Assert.False(await InstanceActivation.TryActivateAsync(Path.Combine(workspace.Root, "Different profile"), TimeSpan.FromMilliseconds(150)));
            Assert.Equal(6, activations);
        }
        Assert.False(await InstanceActivation.TryActivateAsync(workspace.Root, TimeSpan.FromMilliseconds(150)));
        using (var restarted = new InstanceActivation(workspace.Root, () => Interlocked.Increment(ref activations), errors.Enqueue))
            Assert.True(await InstanceActivation.TryActivateAsync(workspace.Root));
        Assert.Equal(7, activations);
        Assert.Empty(errors);
    }

    [Fact]
    public async Task ActivationRestoresTheSameHiddenOrMinimizedWindow()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(DesktopWorkflowTests.TestAppBuilder));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await session.Dispatch(async () =>
        {
            var window = new MainWindow();
            window.Show();
            try
            {
                window.Hide();
                window.RevealExistingInstance();
                Assert.True(window.IsVisible);
                window.WindowState = WindowState.Minimized;
                window.RevealExistingInstance();
                Assert.Equal(WindowState.Normal, window.WindowState);
                window.WindowState = WindowState.Maximized;
                window.RevealExistingInstance();
                Assert.Equal(WindowState.Maximized, window.WindowState);
            }
            finally { await DesktopWindowTest.CloseAsync(window); }
            return true;
        }, timeout.Token);
    }
}
