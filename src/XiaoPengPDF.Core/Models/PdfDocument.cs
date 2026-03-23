namespace XiaoPengPDF.Core.Models;

public class PdfDocument
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public long FileSize { get; set; }
    public DateTime? LastOpened { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
