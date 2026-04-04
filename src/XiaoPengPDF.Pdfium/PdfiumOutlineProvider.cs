using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using XiaoPengPDF.Core.Interfaces;
using XiaoPengPDF.Core.Models;

namespace XiaoPengPDF.Pdfium;

public partial class PdfiumOutlineProvider : IPdfOutlineProvider
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
            Title = GetBookmarkTitle(bookmark),
            PageNumber = GetBookmarkPageNumber(document, bookmark),
            YPosition = 0
        };
    }

    private static string GetBookmarkTitle(IntPtr bookmark)
    {
        int length = FPDFBookmark_GetTitle(bookmark, null, 0);
        if (length == 0)
            return string.Empty;

        byte[] buffer = new byte[length * 2];
        int result = FPDFBookmark_GetTitle(bookmark, buffer, length * 2);
        return result > 0 ? Encoding.Unicode.GetString(buffer, 0, buffer.Length).TrimEnd('\0') : string.Empty;
    }

    private static int GetBookmarkPageNumber(IntPtr document, IntPtr bookmark)
    {
        IntPtr destHandle = FPDFBookmark_GetDest(document, bookmark);
        if (destHandle == IntPtr.Zero)
            return 0;

        return FPDFDest_GetPageIndex(document, destHandle);
    }

    [LibraryImport(PdfiumDll, EntryPoint = "FPDFBookmark_GetCount")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial int FPDFBookmark_GetCount(IntPtr document);

    [LibraryImport(PdfiumDll, EntryPoint = "FPDFBookmark_GetItem")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial IntPtr FPDFBookmark_GetItem(IntPtr document, int index);

    [LibraryImport(PdfiumDll, EntryPoint = "FPDFBookmark_GetTitle", StringMarshalling = StringMarshalling.Utf16)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial int FPDFBookmark_GetTitle(IntPtr bookmark, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[]? buffer, int length);

    [LibraryImport(PdfiumDll, EntryPoint = "FPDFBookmark_GetDest")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial IntPtr FPDFBookmark_GetDest(IntPtr document, IntPtr bookmark);

    [LibraryImport(PdfiumDll, EntryPoint = "FPDFDest_GetPageIndex")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial int FPDFDest_GetPageIndex(IntPtr document, IntPtr dest);
}
