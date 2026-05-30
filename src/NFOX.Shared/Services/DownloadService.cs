using System.Net.Http.Headers;

namespace NFOX.Shared.Services;

public sealed class DownloadService
{
    private readonly HttpClient _httpClient = new();

    public async Task<string> GetStringAsync(string url, CancellationToken cancellationToken)
    {
        if (TryResolveLocalPath(url, out var localPath))
        {
            return await File.ReadAllTextAsync(localPath, cancellationToken);
        }

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task DownloadFileAsync(string url, string destinationPath, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var destinationDirectory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrWhiteSpace(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        if (TryResolveLocalPath(url, out var localPath))
        {
            await CopyLocalFileAsync(localPath, destinationPath, progress, cancellationToken);
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("NFOX-AutoUpdateDemo", "1.0"));
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength;
        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        await CopyStreamAsync(source, destinationPath, totalBytes, progress, cancellationToken);
    }

    private static bool TryResolveLocalPath(string url, out string localPath)
    {
        localPath = "";
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.IsFile)
        {
            localPath = uri.LocalPath;
            return true;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _) && File.Exists(url))
        {
            localPath = Path.GetFullPath(url);
            return true;
        }

        return false;
    }

    private static async Task CopyLocalFileAsync(string sourcePath, string destinationPath, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        await using var source = File.OpenRead(sourcePath);
        await CopyStreamAsync(source, destinationPath, source.Length, progress, cancellationToken);
    }

    private static async Task CopyStreamAsync(Stream source, string destinationPath, long? totalBytes, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        await using var destination = File.Create(destinationPath);
        var buffer = new byte[81920];
        long readBytes = 0;

        while (true)
        {
            var read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (read == 0)
            {
                break;
            }

            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            readBytes += read;

            if (totalBytes.HasValue && totalBytes.Value > 0)
            {
                progress?.Report(readBytes * 100d / totalBytes.Value);
            }
        }

        progress?.Report(100);
    }
}
