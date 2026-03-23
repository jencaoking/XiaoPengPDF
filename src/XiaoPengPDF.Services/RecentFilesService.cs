using System.Text.Json;
using XiaoPengPDF.Infrastructure.Configuration;

namespace XiaoPengPDF.Services;

public class RecentFilesService
{
    private readonly AppSettings _settings;

    public RecentFilesService(AppSettings settings)
    {
        _settings = settings;
    }

    public IReadOnlyList<string> GetRecentFiles() => _settings.RecentFiles.AsReadOnly();

    public void AddRecentFile(string filePath)
    {
        _settings.RecentFiles.RemoveAll(f => f.Equals(filePath, StringComparison.OrdinalIgnoreCase));
        _settings.RecentFiles.Insert(0, filePath);

        if (_settings.RecentFiles.Count > _settings.MaxRecentFiles)
            _settings.RecentFiles = _settings.RecentFiles.Take(_settings.MaxRecentFiles).ToList();

        _settings.Save();
    }

    public void RemoveRecentFile(string filePath)
    {
        _settings.RecentFiles.RemoveAll(f => f.Equals(filePath, StringComparison.OrdinalIgnoreCase));
        _settings.Save();
    }

    public void ClearRecentFiles()
    {
        _settings.RecentFiles.Clear();
        _settings.Save();
    }
}
