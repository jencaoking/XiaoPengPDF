using System.Runtime.InteropServices;
using XiaoPengPDF.Core.Interfaces;
using XiaoPengPDF.Core.Models;

namespace XiaoPengPDF.Pdfium;

public class PdfiumSearchService : IPdfSearchService
{
    private const string PdfiumDll = "pdfium";

    static PdfiumSearchService()
    {
        PdfiumNativeLoader.Initialize();
    }

    public List<PdfSearchResult> Search(IPdfDocument document, string query, bool caseSensitive = false)
    {
        if (document is not PdfiumDocument pdfiumDoc)
            throw new ArgumentException("Document must be a PdfiumDocument", nameof(document));

        var results = new List<PdfSearchResult>();
        for (int i = 0; i < document.PageCount; i++)
        {
            results.AddRange(SearchPage(document, i, query, caseSensitive));
        }
        return results;
    }

    public List<PdfSearchResult> SearchPage(IPdfDocument document, int pageNumber, string query, bool caseSensitive = false)
    {
        if (document is not PdfiumDocument pdfiumDoc)
            throw new ArgumentException("Document must be a PdfiumDocument", nameof(document));

        var results = new List<PdfSearchResult>();
        IntPtr searchHandle = FPDFText_FindStart(pdfiumDoc.Handle, pageNumber, query, caseSensitive ? 0 : 2);

        if (searchHandle == IntPtr.Zero)
            return results;

        try
        {
            while (FPDFText_FindNext(searchHandle))
            {
                int startIndex = FPDFText_GetSchResultIndex(searchHandle);
                int length = FPDFText_GetMatchLength(searchHandle);

                results.Add(new PdfSearchResult
                {
                    PageNumber = pageNumber,
                    StartIndex = startIndex,
                    Length = length,
                    X = 0,
                    Y = 0,
                    Width = 0,
                    Height = 0
                });
            }
        }
        finally
        {
            FPDFText_FindClose(searchHandle);
        }

        return results;
    }

    [DllImport(PdfiumDll, EntryPoint = "FPDFText_FindStart", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr FPDFText_FindStart(IntPtr document, int pageIndex, [MarshalAs(UnmanagedType.LPWStr)] string query, int flags);

    [DllImport(PdfiumDll, EntryPoint = "FPDFText_FindNext", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool FPDFText_FindNext(IntPtr handle);

    [DllImport(PdfiumDll, EntryPoint = "FPDFText_GetSchResultIndex", CallingConvention = CallingConvention.Cdecl)]
    private static extern int FPDFText_GetSchResultIndex(IntPtr handle);

    [DllImport(PdfiumDll, EntryPoint = "FPDFText_GetMatchLength", CallingConvention = CallingConvention.Cdecl)]
    private static extern int FPDFText_GetMatchLength(IntPtr handle);

    [DllImport(PdfiumDll, EntryPoint = "FPDFText_FindClose", CallingConvention = CallingConvention.Cdecl)]
    private static extern void FPDFText_FindClose(IntPtr handle);
}
