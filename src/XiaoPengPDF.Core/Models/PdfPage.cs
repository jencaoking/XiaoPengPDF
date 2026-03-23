namespace XiaoPengPDF.Core.Models;

public class PdfPage
{
    public int PageNumber { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Rotation { get; set; }
    public string? TextContent { get; set; }
}
