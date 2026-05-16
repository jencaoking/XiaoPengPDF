using System.Text.Json;
using XiaoPengPDF.Core;

namespace XiaoPengPDF.Infrastructure.Configuration;

public class AppSettings
{
    public string Theme { get; set; } = AppConstants.DefaultTheme;
    public string Language { get; set; } = AppConstants.DefaultLanguage;
    public double DefaultZoom { get; set; } = AppConstants.DefaultZoom;
    public string DefaultFitMode { get; set; } = AppConstants.DefaultFitMode;
    public bool ShowScrollBar { get; set; } = true;
    public bool RememberLastPosition { get; set; } = true;
    public List<string> RecentFiles { get; set; } = new();
    public int MaxRecentFiles { get; set; } = AppConstants.DefaultMaxRecentFiles;

    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppConstants.AppName);

    private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, AppConstants.SettingsFileName);

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch
        {
        }
        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            if (!Directory.Exists(SettingsDirectory))
            {
                Directory.CreateDirectory(SettingsDirectory);
            }
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }
        catch
        {
        }
    }
}
