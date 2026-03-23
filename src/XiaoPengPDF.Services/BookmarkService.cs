using System.Text.Json;
using XiaoPengPDF.Core.Interfaces;
using XiaoPengPDF.Core.Models;
using XiaoPengPDF.Infrastructure.Logging;

namespace XiaoPengPDF.Services;

public class BookmarkService : IBookmarkService
{
    private readonly string _bookmarksFilePath;

    public BookmarkService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "XiaoPengPDF");

        if (!Directory.Exists(appDataPath))
            Directory.CreateDirectory(appDataPath);

        _bookmarksFilePath = Path.Combine(appDataPath, "bookmarks.json");
    }

    public async Task<List<PdfBookmark>> GetBookmarksAsync(string filePath)
    {
        var bookmarks = await LoadAllBookmarksAsync();
        return bookmarks.Where(b => b.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task AddBookmarkAsync(PdfBookmark bookmark)
    {
        var bookmarks = await LoadAllBookmarksAsync();
        bookmark.Id = bookmarks.Count > 0 ? bookmarks.Max(b => b.Id) + 1 : 1;
        bookmark.CreatedAt = DateTime.Now;
        bookmarks.Add(bookmark);
        await SaveAllBookmarksAsync(bookmarks);
    }

    public async Task RemoveBookmarkAsync(int bookmarkId)
    {
        var bookmarks = await LoadAllBookmarksAsync();
        var bookmark = bookmarks.FirstOrDefault(b => b.Id == bookmarkId);
        if (bookmark != null)
        {
            bookmarks.Remove(bookmark);
            await SaveAllBookmarksAsync(bookmarks);
        }
    }

    public async Task UpdateBookmarkAsync(PdfBookmark bookmark)
    {
        var bookmarks = await LoadAllBookmarksAsync();
        var index = bookmarks.FindIndex(b => b.Id == bookmark.Id);
        if (index >= 0)
        {
            bookmarks[index] = bookmark;
            await SaveAllBookmarksAsync(bookmarks);
        }
    }

    private async Task<List<PdfBookmark>> LoadAllBookmarksAsync()
    {
        if (!File.Exists(_bookmarksFilePath))
            return new List<PdfBookmark>();

        try
        {
            var json = await File.ReadAllTextAsync(_bookmarksFilePath);
            return JsonSerializer.Deserialize<List<PdfBookmark>>(json) ?? new List<PdfBookmark>();
        }
        catch (JsonException ex)
        {
            LoggingService.Error("Failed to parse bookmarks JSON, file may be corrupted", ex);
            return new List<PdfBookmark>();
        }
        catch (Exception ex)
        {
            LoggingService.Error("Failed to load bookmarks", ex);
            return new List<PdfBookmark>();
        }
    }

    private async Task SaveAllBookmarksAsync(List<PdfBookmark> bookmarks)
    {
        var json = JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_bookmarksFilePath, json);
    }
}
