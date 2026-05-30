using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace NFOX.Shared.Services;

public static class ConfigService
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true
    };

    public static T Load<T>(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Missing config file: {path}", path);
        }

        var json = File.ReadAllText(path, Encoding.UTF8);
        return JsonSerializer.Deserialize<T>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Config file is empty or invalid: {path}");
    }

    public static void Save<T>(string path, T value)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(value, ConfigService.JsonOptions);
        File.WriteAllText(path, json, new UTF8Encoding(false));
    }

    public static string ResolvePath(string baseDirectory, string pathOrUri)
    {
        if (string.IsNullOrWhiteSpace(pathOrUri))
        {
            return baseDirectory;
        }

        if (Uri.TryCreate(pathOrUri, UriKind.Absolute, out var uri) && uri.IsFile)
        {
            return uri.LocalPath;
        }

        if (Path.IsPathRooted(pathOrUri))
        {
            return Path.GetFullPath(pathOrUri);
        }

        return Path.GetFullPath(Path.Combine(baseDirectory, pathOrUri));
    }
}
