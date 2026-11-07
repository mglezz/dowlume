using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using Dowlume.Infrastructure;
using Microsoft.Win32;

namespace Dowlume.App.Services;

public sealed record AvailableUpdate(Version Version, Uri ReleasePage, Uri? InstallerUrl, string? Sha256, long Size);
public sealed record DownloadedUpdate(string Path, string Sha256);

public interface IAppUpdates
{
    bool CanInstall
    {
        get;
    }
    Task<AvailableUpdate?> CheckAsync(Version currentVersion, CancellationToken cancellationToken);
    Task<DownloadedUpdate> DownloadAsync(AvailableUpdate update, CancellationToken cancellationToken);
    void InstallAfterExit(DownloadedUpdate update);
    void OpenRelease(AvailableUpdate update);
}

public sealed class AppUpdates(HttpClient? httpClient = null) : IAppUpdates
{
    private const string Repository = "https://github.com/mglezz/dowlume";
    private const long MaximumInstallerSize = 512 * 1024 * 1024;
    private static readonly HttpClient Client = new(new HttpClientHandler { AllowAutoRedirect = false, CheckCertificateRevocationList = true })
    {
        Timeout = TimeSpan.FromMinutes(10),
        MaxResponseContentBufferSize = 1024 * 1024
    };
    private readonly HttpClient _http = httpClient ?? Client;

    public bool CanInstall
    {
        get
        {
            if (!OperatingSystem.IsWindows() || Program.IsCustomDataDirectory)
                return false;
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{A65DE461-2F0E-47BC-8352-3F980674F3DB}_is1");
            return key?.GetValue("InstallLocation") is string location &&
                string.Equals(Path.Combine(location, "Dowlume.App.exe"), Environment.ProcessPath, StringComparison.OrdinalIgnoreCase);
        }
    }

    public async Task<AvailableUpdate?> CheckAsync(Version currentVersion, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(20));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/mglezz/dowlume/releases/latest");
        request.Headers.UserAgent.ParseAdd("Dowlume-UpdateCheck");
        request.Headers.Accept.ParseAdd("application/vnd.github+json");
        using HttpResponseMessage response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
        response.EnsureSuccessStatusCode();
        await using Stream stream = await response.Content.ReadAsStreamAsync(timeout.Token);
        using var buffer = new MemoryStream();
        await CopyBoundedAsync(stream, buffer, 1024 * 1024, timeout.Token);
        using JsonDocument json = JsonDocument.Parse(buffer.ToArray());
        return ParseRelease(json.RootElement, currentVersion);
    }

    internal static AvailableUpdate? ParseRelease(JsonElement release, Version currentVersion)
    {
        if (release.GetProperty("draft").GetBoolean() || release.GetProperty("prerelease").GetBoolean())
            return null;
        string tag = release.GetProperty("tag_name").GetString() ?? string.Empty;
        if (!tag.StartsWith('v') || !Version.TryParse(tag[1..], out Version? version) || version.Build < 0 || version.Revision >= 0 ||
            tag != $"v{version}")
            throw new InvalidDataException("Invalid stable release version.");
        var current = new Version(currentVersion.Major, currentVersion.Minor, Math.Max(0, currentVersion.Build));
        if (version <= current)
            return null;
        var page = new Uri($"{Repository}/releases/tag/{tag}");
        string name = $"Dowlume-{version}-win-x64-setup.exe";
        string expectedUrl = $"{Repository}/releases/download/{tag}/{name}";
        foreach (JsonElement asset in release.GetProperty("assets").EnumerateArray())
        {
            if (asset.GetProperty("name").GetString() != name)
                continue;
            string? digest = asset.TryGetProperty("digest", out JsonElement value) ? value.GetString() : null;
            long size = asset.GetProperty("size").GetInt64();
            if (asset.GetProperty("browser_download_url").GetString() != expectedUrl ||
                digest is null || !digest.StartsWith("sha256:", StringComparison.Ordinal) ||
                digest.Length != 71 || !digest[7..].All(char.IsAsciiHexDigit) || size is <= 0 or > MaximumInstallerSize)
                break;
            return new AvailableUpdate(version, page, new Uri(expectedUrl), digest[7..], size);
        }
        // A release without a verifiable installer can still be viewed, never executed.
        return new AvailableUpdate(version, page, null, null, 0);
    }

    public async Task<DownloadedUpdate> DownloadAsync(AvailableUpdate update, CancellationToken cancellationToken)
    {
        string expectedUrl = $"{Repository}/releases/download/v{update.Version}/Dowlume-{update.Version}-win-x64-setup.exe";
        if (update.InstallerUrl?.AbsoluteUri != expectedUrl || update.Sha256 is null || update.Sha256.Length != 64 ||
            !update.Sha256.All(char.IsAsciiHexDigit) || update.Size is <= 0 or > MaximumInstallerSize)
            throw new InvalidDataException("No verified installer is available.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(10));
        using HttpResponseMessage response = await DownloadResponseAsync(update.InstallerUrl, timeout.Token);
        await using Stream input = await response.Content.ReadAsStreamAsync(timeout.Token);
        string directory = Directory.CreateTempSubdirectory("Dowlume-update-").FullName;
        string path = Path.Combine(directory, $"Dowlume-{update.Version}-win-x64-setup.exe");
        try
        {
            PathSafety.RejectLinks(directory);
            await using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 81920, useAsync: true))
            {
                await CopyBoundedAsync(input, output, update.Size, timeout.Token);
                if (output.Length != update.Size)
                    throw new InvalidDataException("The installer size did not match.");
                output.Position = 0;
                byte[] hash = await SHA256.HashDataAsync(output, timeout.Token);
                if (!Convert.ToHexString(hash).Equals(update.Sha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("The installer checksum did not match.");
            }
            return new DownloadedUpdate(path, update.Sha256);
        }
        catch
        {
            // Only this invocation's file and empty temporary directory are removed.
            File.Delete(path);
            Directory.Delete(directory);
            throw;
        }
    }

    private async Task<HttpResponseMessage> DownloadResponseAsync(Uri url, CancellationToken token)
    {
        for (int attempt = 0; attempt < 4; attempt++)
        {
            if (url.Scheme != "https" || !url.IsDefaultPort || url.UserInfo.Length != 0 ||
                url.Host is not ("github.com" or "release-assets.githubusercontent.com"))
                throw new InvalidDataException("Untrusted download redirect.");
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd("Dowlume-UpdateCheck");
            HttpResponseMessage response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            if (response.StatusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Redirect or HttpStatusCode.SeeOther or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect)
            {
                Uri? location = response.Headers.Location;
                response.Dispose();
                url = location is null ? throw new InvalidDataException("Missing download redirect.") : new Uri(url, location);
                continue;
            }
            try
            {
                response.EnsureSuccessStatusCode();
                return response;
            }
            catch { response.Dispose(); throw; }
        }
        throw new InvalidDataException("Too many download redirects.");
    }

    private static async Task CopyBoundedAsync(Stream input, Stream output, long maximum, CancellationToken token)
    {
        byte[] buffer = new byte[81920];
        long total = 0;
        int count;
        while ((count = await input.ReadAsync(buffer, token)) != 0)
        {
            total += count;
            if (total > maximum)
                throw new InvalidDataException("The update response exceeded its size limit.");
            await output.WriteAsync(buffer.AsMemory(0, count), token);
        }
    }

    public void InstallAfterExit(DownloadedUpdate update)
    {
        if (!CanInstall)
            throw new InvalidOperationException("Direct updates require an installed Windows copy.");
        Program.PendingInstaller = update;
    }

    internal static Process LaunchInstaller(DownloadedUpdate update, params string[] arguments)
    {
        PathSafety.RejectLinks(update.Path);
        using var file = new FileStream(update.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (!Convert.ToHexString(SHA256.HashData(file)).Equals(update.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The installer changed after download.");
        var start = new ProcessStartInfo(update.Path) { UseShellExecute = true };
        foreach (string argument in arguments)
            start.ArgumentList.Add(argument);
        return Process.Start(start) ?? throw new IOException("The installer did not start.");
    }

    public void OpenRelease(AvailableUpdate update)
    {
        var url = new Uri($"{Repository}/releases/tag/v{update.Version}");
        using Process? process = Process.Start(new ProcessStartInfo(url.AbsoluteUri) { UseShellExecute = true });
    }
}
