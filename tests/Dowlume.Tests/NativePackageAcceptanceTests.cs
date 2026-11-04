using Dowlume.App.Services;

namespace Dowlume.Tests;

public sealed class NativePackageAcceptanceTests
{
    [Fact]
    public async Task NormalLaunchCannotRunTheDisposableAcceptanceWorkflow()
    {
        using var workspace = new TemporaryWorkspace();
        string[] originalFiles = Directory.GetFiles(workspace.Root, "*", SearchOption.AllDirectories);

        await Assert.ThrowsAsync<InvalidOperationException>(() => NativePackageAcceptance.RunAsync(null!, workspace.Root));

        Assert.Equal(originalFiles, Directory.GetFiles(workspace.Root, "*", SearchOption.AllDirectories));
        Assert.False(Directory.Exists(Path.Combine(workspace.Root, "Samples")));
    }
}
