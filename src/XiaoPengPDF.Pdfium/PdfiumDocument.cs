using System.Runtime.InteropServices;
using XiaoPengPDF.Core.Interfaces;
using XiaoPengPDF.Core.Models;

namespace XiaoPengPDF.Pdfium;

public class PdfiumDocument : IPdfDocument, IDisposable
{
    private readonly IntPtr _document;
    private bool _disposed;

    public string FilePath { get; }
    public int PageCount { get; }

    internal IntPtr Handle => _document;

    static PdfiumDocument()
    {
        PdfiumNativeLoader.Initialize();
    }

    public PdfiumDocument(string filePath)
    {
        FilePath = filePath;

        _document = FPDF_LoadDocument(filePath, "");

        if (_document == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Failed to load PDF document: {filePath}");
        }

        PageCount = FPDF_GetPageCount(_document);
    }

    public PdfPage GetPage(int pageNumber)
    {
        if (pageNumber < 0 || pageNumber >= PageCount)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));

        var page = new PdfPage
        {
            PageNumber = pageNumber,
            Width = FPDF_GetPageWidth(_document, pageNumber),
            Height = FPDF_GetPageHeight(_document, pageNumber),
            Rotation = FPDF_GetPageRotation(_document, pageNumber)
        };

        return page;
    }

    public void Save()
    {
        FPDF_SaveDocument(_document, FilePath);
    }

    public void Close()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            FPDF_CloseDocument(_document);
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    private const string PdfiumDll = "pdfium";

    [DllImport(PdfiumDll, EntryPoint = "FPDF_LoadDocument", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    private static extern IntPtr FPDF_LoadDocument([MarshalAs(UnmanagedType.LPWStr)] string filePath, [MarshalAs(UnmanagedType.LPWStr)] string password);

    [DllImport(PdfiumDll, EntryPoint = "FPDF_GetPageCount", CallingConvention = CallingConvention.Cdecl)]
    private static extern int FPDF_GetPageCount(IntPtr document);

    [DllImport(PdfiumDll, EntryPoint = "FPDF_GetPageWidth", CallingConvention = CallingConvention.Cdecl)]
    private static extern double FPDF_GetPageWidth(IntPtr document, int pageIndex);

    [DllImport(PdfiumDll, EntryPoint = "FPDF_GetPageHeight", CallingConvention = CallingConvention.Cdecl)]
    private static extern double FPDF_GetPageHeight(IntPtr document, int pageIndex);

    [DllImport(PdfiumDll, EntryPoint = "FPDF_GetPageRotation", CallingConvention = CallingConvention.Cdecl)]
    private static extern int FPDF_GetPageRotation(IntPtr document, int pageIndex);

    [DllImport(PdfiumDll, EntryPoint = "FPDF_SaveDocument", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    private static extern void FPDF_SaveDocument(IntPtr document, [MarshalAs(UnmanagedType.LPWStr)] string filePath);

    [DllImport(PdfiumDll, EntryPoint = "FPDF_CloseDocument", CallingConvention = CallingConvention.Cdecl)]
    private static extern void FPDF_CloseDocument(IntPtr document);
}
