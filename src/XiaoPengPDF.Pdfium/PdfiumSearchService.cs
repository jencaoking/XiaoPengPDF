using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using XiaoPengPDF.Core.Interfaces;
using XiaoPengPDF.Core.Models;

namespace XiaoPengPDF.Pdfium;

public partial class PdfiumSearchService : IPdfSearchService
{
    private const string PdfiumDll = "pdfium";

    static PdfiumSearchService()
    {
        PdfiumNativeLoader.Initialize();
    }

    public List<PdfSearchResult> Search(IPdfDocument document, string query, bool caseSensitive = false)
    {
        if (document is not PdfiumDocument)
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

        IntPtr textPage = FPDFText_LoadPage(pdfiumDoc.Handle, pageNumber);
        if (textPage == IntPtr.Zero)
            return results;

        IntPtr searchHandle = IntPtr.Zero;
        try
        {
            searchHandle = FPDFText_FindStart(textPage, query, caseSensitive ? 0 : 2);
            if (searchHandle == IntPtr.Zero)
                return results;

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
            if (searchHandle != IntPtr.Zero)
                FPDFText_FindClose(searchHandle);
            FPDFText_ClosePage(textPage);
        }

        return results;
    }

    [LibraryImport(PdfiumDll, EntryPoint = "FPDFText_LoadPage")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial IntPtr FPDFText_LoadPage(IntPtr document, int pageIndex);

    [LibraryImport(PdfiumDll, EntryPoint = "FPDFText_ClosePage")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial void FPDFText_ClosePage(IntPtr textPage);

    [LibraryImport(PdfiumDll, EntryPoint = "FPDFText_FindStart", StringMarshalling = StringMarshalling.Utf16)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial IntPtr FPDFText_FindStart(IntPtr textPage, [MarshalAs(UnmanagedType.LPWStr)] string query, int flags);

    [LibraryImport(PdfiumDll, EntryPoint = "FPDFText_FindNext")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool FPDFText_FindNext(IntPtr handle);

    [LibraryImport(PdfiumDll, EntryPoint = "FPDFText_GetSchResultIndex")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial int FPDFText_GetSchResultIndex(IntPtr handle);

    [LibraryImport(PdfiumDll, EntryPoint = "FPDFText_GetMatchLength")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial int FPDFText_GetMatchLength(IntPtr handle);

    [LibraryImport(PdfiumDll, EntryPoint = "FPDFText_FindClose")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial void FPDFText_FindClose(IntPtr handle);
}
