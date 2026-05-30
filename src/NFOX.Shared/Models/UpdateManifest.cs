using System.Text.Json.Serialization;

namespace NFOX.Shared.Models;

public sealed class UpdateManifest
{
    [JsonPropertyName("appName")]
    public string AppName { get; set; } = "";

    [JsonPropertyName("updateName")]
    public string UpdateName { get; set; } = "";

    [JsonPropertyName("latestAppVersion")]
    public string LatestAppVersion { get; set; } = "";

    [JsonPropertyName("minimumRequiredAppVersion")]
    public string MinimumRequiredAppVersion { get; set; } = "";

    [JsonPropertyName("targetDbVersion")]
    public string TargetDbVersion { get; set; } = "";

    [JsonPropertyName("isRequired")]
    public bool IsRequired { get; set; }

    [JsonPropertyName("releaseNotes")]
    public string ReleaseNotes { get; set; } = "";

    [JsonPropertyName("publishedAt")]
    public DateTimeOffset PublishedAt { get; set; }

    [JsonPropertyName("packages")]
    public UpdatePackages Packages { get; set; } = new();
}

public sealed class UpdatePackages
{
    [JsonPropertyName("updatePackage")]
    public PackageInfo? UpdatePackage { get; set; }

    [JsonPropertyName("app")]
    public PackageInfo App { get; set; } = new();

    [JsonPropertyName("migrations")]
    public PackageInfo Migrations { get; set; } = new();
}
