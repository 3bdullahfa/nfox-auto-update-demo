using System.Text;

namespace NFOX.Shared.Services;

public sealed class LogService
{
    private readonly object _syncRoot = new();
    private readonly string _category;

    public LogService(string category, string? logDirectory = null)
    {
        _category = category;
        LogDirectory = logDirectory ?? Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(LogDirectory);
    }

    public string LogDirectory { get; }

    public void Info(string operation, string message) => Write("INFO", operation, message, null);

    public void Warning(string operation, string message) => Write("WARN", operation, message, null);

    public void Error(string operation, string message, Exception? exception = null) => Write("ERROR", operation, message, exception);

    private void Write(string level, string operation, string message, Exception? exception)
    {
        var filePath = Path.Combine(LogDirectory, $"{_category}-{DateTime.Now:yyyy-MM-dd}.log");
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {operation} - {message}";
        if (exception is not null)
        {
            line += Environment.NewLine + exception;
        }

        lock (_syncRoot)
        {
            File.AppendAllText(filePath, line + Environment.NewLine, new UTF8Encoding(false));
        }
    }
}
