using System.IO.Compression;

namespace NFOX.Shared.Services;

public static class ZipService
{
    public static void ExtractToDirectory(string zipPath, string destinationDirectory, bool cleanDestination)
    {
        if (cleanDestination && Directory.Exists(destinationDirectory))
        {
            Directory.Delete(destinationDirectory, true);
        }

        Directory.CreateDirectory(destinationDirectory);
        ZipFile.ExtractToDirectory(zipPath, destinationDirectory, true);
    }

    public static void CopyDirectory(string sourceDirectory, string destinationDirectory, bool overwrite)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDirectory}");
        }

        Directory.CreateDirectory(destinationDirectory);

        foreach (var directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, directory);
            Directory.CreateDirectory(Path.Combine(destinationDirectory, relative));
        }

        foreach (var file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, file);
            var destination = Path.Combine(destinationDirectory, relative);
            var parent = Path.GetDirectoryName(destination);
            if (!string.IsNullOrWhiteSpace(parent))
            {
                Directory.CreateDirectory(parent);
            }

            File.Copy(file, destination, overwrite);
        }
    }
}
