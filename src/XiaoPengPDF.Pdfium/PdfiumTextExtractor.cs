using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using XiaoPengPDF.Core.Interfaces;

namespace XiaoPengPDF.Pdfium;

public partial class PdfiumTextExtractor : IPdfTextExtractor
{
    private const string PdfiumDll = "pdfium";

    static PdfiumTextExtractor()
    {
        PdfiumNativeLoader.Initialize();
    }

    public string ExtractText(IPdfDocument document, int pageNumber)
    {
        if (document is not PdfiumDocument pdfiumDoc)
            throw new ArgumentException("Document must be a PdfiumDocument", nameof(document));

        IntPtr textHandle = FPDFText_GetPageText(pdfiumDoc.Handle, pageNumber);
        if (textHandle == IntPtr.Zero)
            return string.Empty;

        try
        {
            return GetStringFromPtr(textHandle);
        }
        finally
        {
            FPDFText_ClosePageText(textHandle);
        }
    }

    public string ExtractAllText(IPdfDocument document)
    {
        if (document is not PdfiumDocument)
            throw new ArgumentException("Document must be a PdfiumDocument", nameof(document));

        var sb = new StringBuilder();
        for (int i = 0; i < document.PageCount; i++)
        {
            sb.Append(ExtractText(document, i));
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string GetStringFromPtr(IntPtr handle)
    {
        int length = FPDFText_GetTextLength(handle);
        if (length == 0)
            return string.Empty;

        byte[] buffer = new byte[length * 2];
        int result = FPDFText_GetText(handle, buffer, length * 2);
        return result > 0 ? Encoding.Unicode.GetString(buffer, 0, buffer.Length).TrimEnd('\0') : string.Empty;
    }

    [LibraryImport(PdfiumDll, EntryPoint = "FPDFText_LoadPage")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial IntPtr FPDFText_GetPageText(IntPtr document, int pageIndex);

    [LibraryImport(PdfiumDll, EntryPoint = "FPDFText_ClosePageText")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial void FPDFText_ClosePageText(IntPtr textPage);

    [LibraryImport(PdfiumDll, EntryPoint = "FPDFText_GetTextLength")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial int FPDFText_GetTextLength(IntPtr textPage);

    [LibraryImport(PdfiumDll, EntryPoint = "FPDFText_GetText", StringMarshalling = StringMarshalling.Utf16)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    private static partial int FPDFText_GetText(IntPtr textPage, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] buffer, int length);
}
