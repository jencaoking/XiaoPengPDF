using System.Runtime.InteropServices;
using XiaoPengPDF.Core.Interfaces;
using XiaoPengPDF.Core.Models;

namespace XiaoPengPDF.Pdfium;

public class PdfiumOutlineProvider : IPdfOutlineProvider
{
    private const string PdfiumDll = "pdfium";

    static PdfiumOutlineProvider()
    {
        PdfiumNativeLoader.Initialize();
    }

    public List<PdfOutline> GetOutline(IPdfDocument document)
    {
        if (document is not PdfiumDocument pdfiumDoc)
            throw new ArgumentException("Document must be a PdfiumDocument", nameof(document));

        return GetOutlineInternal(pdfiumDoc.Handle);
    }

    private static List<PdfOutline> GetOutlineInternal(IntPtr document)
    {
        var outlines = new List<PdfOutline>();
        int count = FPDFBookmark_GetCount(document);

        for (int i = 0; i < count; i++)
        {
            outlines.Add(GetOutlineItem(document, i));
        }

        return outlines;
    }

    private static PdfOutline GetOutlineItem(IntPtr document, int index)
    {
        IntPtr bookmark = FPDFBookmark_GetItem(document, index);
        if (bookmark == IntPtr.Zero)
            return new PdfOutline();

        return new PdfOutline
        {
            Title = FPDFBookmark_GetTitle(bookmark),
            PageNumber = FPDFBookmark_GetPageNumber(bookmark),
            YPosition = 0
        };
    }

    [DllImport(PdfiumDll, EntryPoint = "FPDFBookmark_GetCount", CallingConvention = CallingConvention.Cdecl)]
    private static extern int FPDFBookmark_GetCount(IntPtr document);

    [DllImport(PdfiumDll, EntryPoint = "FPDFBookmark_GetItem", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr FPDFBookmark_GetItem(IntPtr document, int index);

    [DllImport(PdfiumDll, EntryPoint = "FPDFBookmark_GetTitle", CallingConvention = CallingConvention.Cdecl)]
    private static extern string FPDFBookmark_GetTitle(IntPtr bookmark);

    [DllImport(PdfiumDll, EntryPoint = "FPDFBookmark_GetPageNumber", CallingConvention = CallingConvention.Cdecl)]
    private static extern int FPDFBookmark_GetPageNumber(IntPtr bookmark);
}
