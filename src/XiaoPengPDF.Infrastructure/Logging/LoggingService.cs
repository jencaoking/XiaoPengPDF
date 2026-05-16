using XiaoPengPDF.Core;

namespace XiaoPengPDF.Infrastructure.Logging;

public class LoggingService
{
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppConstants.AppName,
        AppConstants.LogsDirectoryName);

    public static void Initialize()
    {
        if (!Directory.Exists(LogDirectory))
        {
            Directory.CreateDirectory(LogDirectory);
        }
    }

    public static string GetLogFilePath() => Path.Combine(LogDirectory, string.Format(AppConstants.LogFileNameFormat, DateTime.Now));

    public static void Log(string level, string message, Exception? exception = null)
    {
        var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        if (exception != null)
        {
            logMessage += $"\nException: {exception}";
        }
        File.AppendAllText(GetLogFilePath(), logMessage + Environment.NewLine);
    }

    public static void Info(string message) => Log("INFO", message);
    public static void Warning(string message) => Log("WARNING", message);
    public static void Error(string message, Exception? ex = null) => Log("ERROR", message, ex);
}
