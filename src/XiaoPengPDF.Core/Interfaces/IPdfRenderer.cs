using XiaoPengPDF.Core.Models;

namespace XiaoPengPDF.Core.Interfaces;

public interface IPdfRenderer : IDisposable
{
    byte[] RenderPage(IPdfDocument document, int pageNumber, int width, int height, double scale);
    byte[] RenderPage(IPdfDocument document, int pageNumber, int width, int height, double scale, int rotation);
    byte[] RenderThumbnail(IPdfDocument document, int pageNumber, int width, int height);
}
