namespace XiaoPengPDF.Core.Interfaces;

public interface IPdfTextExtractor
{
    string ExtractText(IPdfDocument document, int pageNumber);
    string ExtractAllText(IPdfDocument document);
}
