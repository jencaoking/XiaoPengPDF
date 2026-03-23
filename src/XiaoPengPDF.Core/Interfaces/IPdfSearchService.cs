using XiaoPengPDF.Core.Models;

namespace XiaoPengPDF.Core.Interfaces;

public interface IPdfSearchService
{
    List<PdfSearchResult> Search(IPdfDocument document, string query, bool caseSensitive = false);
    List<PdfSearchResult> SearchPage(IPdfDocument document, int pageNumber, string query, bool caseSensitive = false);
}
