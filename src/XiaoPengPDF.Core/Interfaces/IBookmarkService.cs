using XiaoPengPDF.Core.Models;

namespace XiaoPengPDF.Core.Interfaces;

public interface IBookmarkService
{
    Task<List<PdfBookmark>> GetBookmarksAsync(string filePath);
    Task AddBookmarkAsync(PdfBookmark bookmark);
    Task RemoveBookmarkAsync(int bookmarkId);
    Task UpdateBookmarkAsync(PdfBookmark bookmark);
}
