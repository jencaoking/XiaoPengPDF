namespace XiaoPengPDF.Core.Models;

public class PdfOutline
{
    public string Title { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public double YPosition { get; set; }
    public List<PdfOutline> Children { get; set; } = new();
}
