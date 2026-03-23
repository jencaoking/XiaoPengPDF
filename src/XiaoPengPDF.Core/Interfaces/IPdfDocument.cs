using XiaoPengPDF.Core.Models;

namespace XiaoPengPDF.Core.Interfaces;

public interface IPdfDocument : IDisposable
{
    string FilePath { get; }
    int PageCount { get; }
    PdfPage GetPage(int pageNumber);
    void Save();
    void Close();
}
