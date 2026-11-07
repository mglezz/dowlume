using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using Dowlume.App.Services;
using Microsoft.Win32;

// Invoked only by package acceptance after it creates a disposable registered installation.
if (!OperatingSystem.IsWindows() || args.Length != 3)
    throw new ArgumentException("Usage: UpdateAcceptance <candidate-installer> <disposable-install-directory> <report>");
string installer = Path.GetFullPath(args[0]);
string destination = Path.GetFullPath(args[1]);
string testRoot = Path.Combine(Path.GetTempPath(), "Dowlume.PackageAcceptance") + Path.DirectorySeparatorChar;
if (!destination.StartsWith(testRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(Path.Combine(destination, "Dowlume.App.exe")))
    throw new InvalidOperationException("A package-acceptance installation is required.");
using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{A65DE461-2F0E-47BC-8352-3F980674F3DB}_is1"))
{
    if (key?.GetValue("InstallLocation") is not string registered ||
        !string.Equals(Path.TrimEndingDirectorySeparator(registered), destination, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("The disposable installation is not registered.");
}
if (Mutex.TryOpenExisting(@"Local\Dowlume.Desktop", out Mutex? running))
{
    running.Dispose();
    throw new InvalidOperationException("Dowlume must exit before update handoff.");
}
byte[] bytes = await File.ReadAllBytesAsync(installer);
string hash = Convert.ToHexString(SHA256.HashData(bytes));
string version = typeof(AppUpdates).Assembly.GetName().Version!.ToString(3);
string url = $"https://github.com/mglezz/dowlume/releases/download/v{version}/Dowlume-{version}-win-x64-setup.exe";
string metadata = JsonSerializer.Serialize(new
{
    tag_name = $"v{version}",
    draft = false,
    prerelease = false,
    assets = new[] { new { name = $"Dowlume-{version}-win-x64-setup.exe", size = bytes.Length, digest = "sha256:" + hash, browser_download_url = url } }
});
using var http = new HttpClient(new FixtureHttpHandler(metadata, bytes, url));
var service = new AppUpdates(http);
AvailableUpdate update = await service.CheckAsync(new Version(0, 5, 0), CancellationToken.None)
    ?? throw new InvalidOperationException("The candidate was not offered as an upgrade.");
DownloadedUpdate downloaded = await service.DownloadAsync(update, CancellationToken.None);
try
{
    using var process = AppUpdates.LaunchInstaller(downloaded, "/VERYSILENT", "/SUPPRESSMSGBOXES", "/NORESTART", "/SP-",
        "/NOFORCECLOSEAPPLICATIONS", $"/DIR={destination}", $"/LOG={Path.Combine(Path.GetDirectoryName(destination)!, "verified-update.log")}");
    using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
    await process.WaitForExitAsync(timeout.Token);
    if (process.ExitCode != 0)
        throw new InvalidOperationException($"Verified installer exited with code {process.ExitCode}.");
    if (System.Diagnostics.FileVersionInfo.GetVersionInfo(Path.Combine(destination, "Dowlume.App.dll")).FileVersion != version + ".0")
        throw new InvalidOperationException("The installed application did not reach the candidate version.");
    await File.WriteAllTextAsync(args[2], JsonSerializer.Serialize(new
    {
        Passed = true,
        Version = version,
        InstallerSha256 = hash,
        ControlledReleaseMetadata = true,
        ProductionDownloadAndHashVerification = true,
        ProductionInstallerLaunch = true,
        ApplicationMutexAbsent = true,
        ManualDesktopAcceptance = false
    }, new JsonSerializerOptions { WriteIndented = true }));
}
finally
{
    File.Delete(downloaded.Path);
    Directory.Delete(Path.GetDirectoryName(downloaded.Path)!);
}

internal sealed class FixtureHttpHandler(string metadata, byte[] bytes, string assetUrl) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpContent content = request.RequestUri!.AbsoluteUri switch
        {
            "https://api.github.com/repos/mglezz/dowlume/releases/latest" => new StringContent(metadata),
            var url when url == assetUrl => new ByteArrayContent(bytes),
            _ => throw new InvalidOperationException("Unexpected acceptance request.")
        };
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
    }
}
