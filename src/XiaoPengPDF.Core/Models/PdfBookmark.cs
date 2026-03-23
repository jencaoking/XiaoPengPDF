namespace XiaoPengPDF.Core.Models;

public class PdfBookmark
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
