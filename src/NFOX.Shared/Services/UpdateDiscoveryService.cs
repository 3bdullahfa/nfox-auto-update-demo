using System.Text.Json;
using System.Text.Json.Serialization;
using NFOX.Shared.Models;

namespace NFOX.Shared.Services;

public sealed class UpdateDiscoveryService
{
    private readonly DownloadService _downloadService = new();

    public Task<UpdateManifest> GetManifestAsync(AppConfig config, CancellationToken cancellationToken)
    {
        return GetManifestAsync(
            config.UpdateSource,
            config.GitHubOwner,
            config.GitHubRepo,
            config.GitHubUseLatestRelease,
            config.ManifestUrl,
            cancellationToken);
    }

    public Task<UpdateManifest> GetManifestAsync(UpdaterConfig config, CancellationToken cancellationToken)
    {
        return GetManifestAsync(
            config.UpdateSource,
            config.GitHubOwner,
            config.GitHubRepo,
            config.GitHubUseLatestRelease,
            config.ManifestUrl,
            cancellationToken);
    }

    public async Task<UpdateManifest> GetManifestAsync(
        string updateSource,
        string githubOwner,
        string githubRepo,
        bool githubUseLatestRelease,
        string manifestUrl,
        CancellationToken cancellationToken)
    {
        var source = updateSource.Trim();
        if (source.Equals("GitHub", StringComparison.OrdinalIgnoreCase) && githubUseLatestRelease)
        {
            var latestReleaseUrl = $"https://api.github.com/repos/{githubOwner}/{githubRepo}/releases/latest";
            var releaseJson = await _downloadService.GetStringAsync(latestReleaseUrl, cancellationToken);
            var release = JsonSerializer.Deserialize<GitHubReleaseResponse>(releaseJson, ConfigService.JsonOptions)
                ?? throw new InvalidOperationException("GitHub latest release response is empty or invalid.");
            var manifestAsset = release.Assets.FirstOrDefault(asset => asset.Name.Equals("manifest.json", StringComparison.OrdinalIgnoreCase));
            if (manifestAsset is null || string.IsNullOrWhiteSpace(manifestAsset.BrowserDownloadUrl))
            {
                throw new InvalidOperationException("Latest GitHub release does not include a manifest.json asset.");
            }

            return await LoadManifestFromUrlAsync(manifestAsset.BrowserDownloadUrl, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(manifestUrl) ||
            manifestUrl.Contains("PUT_GITHUB_RELEASE_MANIFEST_URL_HERE", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("ManifestUrl is not configured.");
        }

        return await LoadManifestFromUrlAsync(manifestUrl, cancellationToken);
    }

    private async Task<UpdateManifest> LoadManifestFromUrlAsync(string manifestUrl, CancellationToken cancellationToken)
    {
        var manifestJson = await _downloadService.GetStringAsync(manifestUrl, cancellationToken);
        return JsonSerializer.Deserialize<UpdateManifest>(manifestJson, ConfigService.JsonOptions)
            ?? throw new InvalidOperationException("Manifest JSON is empty or invalid.");
    }

    private sealed class GitHubReleaseResponse
    {
        [JsonPropertyName("assets")]
        public List<GitHubReleaseAsset> Assets { get; set; } = new();
    }

    private sealed class GitHubReleaseAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; } = "";
    }
}
