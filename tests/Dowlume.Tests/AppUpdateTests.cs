using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Dowlume.App.Services;

namespace Dowlume.Tests;

public sealed class AppUpdateTests
{
    [Theory]
    [InlineData("v0.5.0", false, false, false)]
    [InlineData("v0.4.0", false, false, false)]
    [InlineData("v0.6.0", false, false, true)]
    [InlineData("v0.10.0", false, false, true)]
    [InlineData("v0.6.0", true, false, false)]
    [InlineData("v0.6.0", false, true, false)]
    public void OnlyNewStableReleasesAreOffered(string tag, bool draft, bool prerelease, bool available)
    {
        using JsonDocument json = Release(tag, draft: draft, prerelease: prerelease);
        Assert.Equal(available, AppUpdates.ParseRelease(json.RootElement, new Version(0, 5, 0, 0)) is not null);
    }

    [Theory]
    [InlineData("v0.6.0-rc.1")]
    [InlineData("v0.6")]
    [InlineData("v0.6.0.1")]
    [InlineData("v0.6.0/evil")]
    [InlineData("v00.6.0")]
    public void InvalidStableTagsAreRejected(string tag)
    {
        using JsonDocument json = Release(tag);
        Assert.Throws<InvalidDataException>(() => AppUpdates.ParseRelease(json.RootElement, new Version(0, 5, 0)));
    }

    [Theory]
    [InlineData(null, null, 10)]
    [InlineData("sha256:invalid", null, 10)]
    [InlineData("valid", "https://example.com/installer.exe", 10)]
    [InlineData("valid", null, 0)]
    [InlineData("valid", null, 536870913)]
    public void UnverifiableAssetsOnlyOfferTheOfficialReleasePage(string? digest, string? url, long size)
    {
        using JsonDocument json = Release("v0.6.0", digest: digest == "valid" ? "sha256:" + new string('a', 64) : digest, url: url, size: size);
        AvailableUpdate update = AppUpdates.ParseRelease(json.RootElement, new Version(0, 5, 0))!;
        Assert.Equal("https://github.com/mglezz/dowlume/releases/tag/v0.6.0", update.ReleasePage.AbsoluteUri);
        Assert.Null(update.InstallerUrl);
    }

    [Fact]
    public async Task CheckUsesOnlyThePublicEndpointWithoutCredentialsOrPersonalData()
    {
        using JsonDocument json = Release("v0.6.0");
        using var http = new HttpClient(new Handler(request =>
        {
            Assert.Equal("https://api.github.com/repos/mglezz/dowlume/releases/latest", request.RequestUri!.AbsoluteUri);
            Assert.Null(request.Headers.Authorization);
            Assert.Null(request.Content);
            Assert.Equal("Dowlume-UpdateCheck", request.Headers.UserAgent.ToString());
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json.RootElement.GetRawText()) };
        }));
        Assert.NotNull(await new AppUpdates(http).CheckAsync(new Version(0, 5, 0), CancellationToken.None));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task DownloadRequiresMatchingSizeAndHash(bool matchingHash, bool oversized)
    {
        byte[] content = Encoding.UTF8.GetBytes("inert installer fixture; never executed");
        string hash = Convert.ToHexString(SHA256.HashData(content));
        var update = new AvailableUpdate(new Version(0, 6, 0), new Uri("https://github.com/mglezz/dowlume/releases/tag/v0.6.0"),
            new Uri("https://github.com/mglezz/dowlume/releases/download/v0.6.0/Dowlume-0.6.0-win-x64-setup.exe"),
            matchingHash ? hash : new string('0', 64), oversized ? content.Length - 1 : content.Length);
        using var http = new HttpClient(new Handler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(content) }));
        var service = new AppUpdates(http);
        if (!matchingHash || oversized)
        {
            await Assert.ThrowsAsync<InvalidDataException>(() => service.DownloadAsync(update, CancellationToken.None));
            return;
        }
        DownloadedUpdate downloaded = await service.DownloadAsync(update, CancellationToken.None);
        try
        {
            Assert.Equal(content, await File.ReadAllBytesAsync(downloaded.Path));
            Assert.Equal(hash, downloaded.Sha256);
            // Launch must reject tampering before attempting to execute this inert fixture.
            await File.WriteAllTextAsync(downloaded.Path, "changed");
            Assert.Throws<InvalidDataException>(() => AppUpdates.LaunchInstaller(downloaded));
        }
        finally
        {
            File.Delete(downloaded.Path);
            Directory.Delete(Path.GetDirectoryName(downloaded.Path)!);
        }
    }

    [Theory]
    [InlineData("http://release-assets.githubusercontent.com/fixture")]
    [InlineData("https://example.com/fixture")]
    [InlineData("https://github.com:444/fixture")]
    [InlineData("https://user@github.com/fixture")]
    public async Task UntrustedRedirectsAreRejectedBeforeAnotherRequest(string location)
    {
        int calls = 0;
        using var http = new HttpClient(new Handler(_ =>
        {
            calls++;
            var response = new HttpResponseMessage(HttpStatusCode.Redirect);
            response.Headers.Location = new Uri(location);
            return response;
        }));
        using JsonDocument json = Release("v0.6.0", digest: "sha256:" + new string('a', 64));
        var update = AppUpdates.ParseRelease(json.RootElement, new Version(0, 5, 0))!;
        await Assert.ThrowsAsync<InvalidDataException>(() => new AppUpdates(http).DownloadAsync(update, CancellationToken.None));
        Assert.Equal(1, calls);
    }

    private static JsonDocument Release(string tag, bool draft = false, bool prerelease = false, string? digest = null, string? url = null, long size = 10) =>
        JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            tag_name = tag,
            draft,
            prerelease,
            assets = new[] { new { name = $"Dowlume-{tag[1..]}-win-x64-setup.exe", digest, size,
                browser_download_url = url ?? $"https://github.com/mglezz/dowlume/releases/download/{tag}/Dowlume-{tag[1..]}-win-x64-setup.exe" } }
        }));

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(respond(request));
    }
}
