using System.Runtime.InteropServices;
using XiaoPengPDF.Core.Interfaces;
using XiaoPengPDF.Core.Models;

namespace XiaoPengPDF.Pdfium;

public class PdfiumRenderer : IPdfRenderer
{
    private const string PdfiumDll = "pdfium";
    private bool _disposed;

    static PdfiumRenderer()
    {
        PdfiumNativeLoader.Initialize();
    }

    public byte[] RenderPage(IPdfDocument document, int pageNumber, int width, int height, double scale)
    {
        return RenderPage(document, pageNumber, width, height, scale, 0);
    }

    public byte[] RenderPage(IPdfDocument document, int pageNumber, int width, int height, double scale, int rotation)
    {
        if (document is not PdfiumDocument pdfiumDoc)
            throw new ArgumentException("Document must be a PdfiumDocument", nameof(document));

        int renderWidth = (int)(width * scale);
        int renderHeight = (int)(height * scale);

        byte[] buffer = new byte[renderWidth * renderHeight * 4];

        FPDF_RenderPageBitmap(pdfiumDoc.Handle, pageNumber, buffer, renderWidth, renderHeight, rotation);

        return ConvertBgraToRgba(buffer, renderWidth, renderHeight);
    }

    public byte[] RenderThumbnail(IPdfDocument document, int pageNumber, int width, int height)
    {
        if (document is not PdfiumDocument pdfiumDoc)
            throw new ArgumentException("Document must be a PdfiumDocument", nameof(document));

        byte[] buffer = new byte[width * height * 4];
        FPDF_RenderPageThumbnail(pdfiumDoc.Handle, pageNumber, buffer, width, height);
        return ConvertBgraToRgba(buffer, width, height);
    }

    private static byte[] ConvertBgraToRgba(byte[] buffer, int width, int height)
    {
        byte[] result = new byte[buffer.Length];
        for (int i = 0; i < buffer.Length; i += 4)
        {
            result[i] = buffer[i + 2];
            result[i + 1] = buffer[i + 1];
            result[i + 2] = buffer[i];
            result[i + 3] = buffer[i + 3];
        }
        return result;
    }

    [DllImport(PdfiumDll, EntryPoint = "FPDF_RenderPageBitmap", CallingConvention = CallingConvention.Cdecl)]
    private static extern void FPDF_RenderPageBitmap(
        IntPtr document,
        int pageIndex,
        [Out] byte[] buffer,
        int width,
        int height,
        int rotation);

    [DllImport(PdfiumDll, EntryPoint = "FPDF_RenderPageThumbnail", CallingConvention = CallingConvention.Cdecl)]
    private static extern void FPDF_RenderPageThumbnail(
        IntPtr document,
        int pageIndex,
        [Out] byte[] buffer,
        int width,
        int height);

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
