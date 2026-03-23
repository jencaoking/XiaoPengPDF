using XiaoPengPDF.Core.Models;

namespace XiaoPengPDF.Core.Interfaces;

public interface IPdfOutlineProvider
{
    List<PdfOutline> GetOutline(IPdfDocument document);
}
