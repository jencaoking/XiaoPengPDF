using System.Text.Json;

namespace XiaoPengPDF.Infrastructure.Configuration;

public class AppSettings
{
    public string Theme { get; set; } = "Light";
    public string Language { get; set; } = "en-US";
    public double DefaultZoom { get; set; } = 1.0;
    public string DefaultFitMode { get; set; } = "FitWidth";
    public bool ShowScrollBar { get; set; } = true;
    public bool RememberLastPosition { get; set; } = true;
    public List<string> RecentFiles { get; set; } = new();
    public int MaxRecentFiles { get; set; } = 10;

    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "XiaoPengPDF");

    private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "settings.json");

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
