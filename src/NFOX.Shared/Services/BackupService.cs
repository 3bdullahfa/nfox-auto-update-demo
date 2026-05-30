namespace NFOX.Shared.Services;

public sealed class BackupService
{
    public string CreateBackup(string sourceDirectory, string backupRoot)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            throw new DirectoryNotFoundException($"Install directory not found: {sourceDirectory}");
        }

        Directory.CreateDirectory(backupRoot);
        var backupDirectory = Path.Combine(backupRoot, $"NFOX.DemoApp-{DateTime.Now:yyyyMMdd-HHmmss}");
        ZipService.CopyDirectory(sourceDirectory, backupDirectory, true);
        return backupDirectory;
    }
}
