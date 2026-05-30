using System.Diagnostics;

namespace NFOX.Shared.Services;

public static class ProcessService
{
    public static void StartProcess(string fileName, string? arguments = null, string? workingDirectory = null)
    {
        var info = new ProcessStartInfo(fileName)
        {
            UseShellExecute = true,
            WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(fileName) ?? AppContext.BaseDirectory
        };

        if (!string.IsNullOrWhiteSpace(arguments))
        {
            info.Arguments = arguments;
        }

        Process.Start(info);
    }

    public static void OpenFolder(string directory)
    {
        Directory.CreateDirectory(directory);
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{directory}\"") { UseShellExecute = true });
    }

    public static bool IsProcessRunning(string executableName)
    {
        var processName = Path.GetFileNameWithoutExtension(executableName);
        return Process.GetProcessesByName(processName).Any(process => !process.HasExited);
    }

    public static bool WaitForProcessExit(string executableName, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < deadline)
        {
            if (!IsProcessRunning(executableName))
            {
                return true;
            }

            Thread.Sleep(500);
        }

        return !IsProcessRunning(executableName);
    }
}
