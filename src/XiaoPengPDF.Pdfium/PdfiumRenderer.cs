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

        IntPtr page = FPDF_LoadPage(pdfiumDoc.Handle, pageNumber);
        if (page == IntPtr.Zero)
            throw new InvalidOperationException($"Failed to load page {pageNumber}");

        try
        {
            IntPtr bitmap = FPDFBitmap_Create(renderWidth, renderHeight, 0);
            if (bitmap == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create bitmap");

            try
            {
                FPDF_RenderPageBitmap(bitmap, page, 0, 0, renderWidth, renderHeight, rotation);

                byte[] buffer = FPDFBitmap_GetBuffer(bitmap, renderWidth, renderHeight);
                return ConvertBgraToRgba(buffer, renderWidth, renderHeight);
            }
            finally
            {
                FPDFBitmap_Destroy(bitmap);
            }
        }
        finally
        {
            FPDF_ClosePage(page);
        }
    }

    public byte[] RenderThumbnail(IPdfDocument document, int pageNumber, int width, int height)
    {
        if (document is not PdfiumDocument pdfiumDoc)
            throw new ArgumentException("Document must be a PdfiumDocument", nameof(document));

        IntPtr page = FPDF_LoadPage(pdfiumDoc.Handle, pageNumber);
        if (page == IntPtr.Zero)
            throw new InvalidOperationException($"Failed to load page {pageNumber}");

        try
        {
            IntPtr bitmap = FPDFBitmap_Create(width, height, 0);
            if (bitmap == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create bitmap");

            try
            {
                FPDF_RenderPageBitmap(bitmap, page, 0, 0, width, height, 0);
                byte[] buffer = FPDFBitmap_GetBuffer(bitmap, width, height);
                return ConvertBgraToRgba(buffer, width, height);
            }
            finally
            {
                FPDFBitmap_Destroy(bitmap);
            }
        }
        finally
        {
            FPDF_ClosePage(page);
        }
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

    [DllImport(PdfiumDll, EntryPoint = "FPDF_LoadPage", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr FPDF_LoadPage(IntPtr document, int pageIndex);

    [DllImport(PdfiumDll, EntryPoint = "FPDF_ClosePage", CallingConvention = CallingConvention.Cdecl)]
    private static extern void FPDF_ClosePage(IntPtr page);

    [DllImport(PdfiumDll, EntryPoint = "FPDFBitmap_Create", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr FPDFBitmap_Create(int width, int height, int flags);

    [DllImport(PdfiumDll, EntryPoint = "FPDFBitmap_Destroy", CallingConvention = CallingConvention.Cdecl)]
    private static extern void FPDFBitmap_Destroy(IntPtr bitmap);

    [DllImport(PdfiumDll, EntryPoint = "FPDF_RenderPageBitmap", CallingConvention = CallingConvention.Cdecl)]
    private static extern void FPDF_RenderPageBitmap(
        IntPtr bitmap,
        IntPtr page,
        int start_x,
        int start_y,
        int size_x,
        int size_y,
        int rotation);

    [DllImport(PdfiumDll, EntryPoint = "FPDFBitmap_GetBuffer", CallingConvention = CallingConvention.Cdecl)]
    private static extern byte[] FPDFBitmap_GetBuffer(IntPtr bitmap, int width, int height);

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
