using System.Runtime.InteropServices;
using XiaoPengPDF.Core.Interfaces;

namespace XiaoPengPDF.Pdfium;

public class PdfiumTextExtractor : IPdfTextExtractor
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

        return FPDF_Text_GetPageText(pdfiumDoc.Handle, pageNumber);
    }

    public string ExtractAllText(IPdfDocument document)
    {
        if (document is not PdfiumDocument pdfiumDoc)
            throw new ArgumentException("Document must be a PdfiumDocument", nameof(document));

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < document.PageCount; i++)
        {
            sb.Append(FPDF_Text_GetPageText(pdfiumDoc.Handle, i));
            sb.AppendLine();
        }
        return sb.ToString();
    }

    [DllImport(PdfiumDll, EntryPoint = "FPDF_Text_GetPageText", CallingConvention = CallingConvention.Cdecl)]
    private static extern string FPDF_Text_GetPageText(IntPtr document, int pageIndex);
}
