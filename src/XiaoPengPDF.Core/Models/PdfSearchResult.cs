namespace XiaoPengPDF.Core.Models;

public class PdfSearchResult
{
    public int PageNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public int StartIndex { get; set; }
    public int Length { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}
