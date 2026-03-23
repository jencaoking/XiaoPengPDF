using XiaoPengPDF.Core.Interfaces;
using XiaoPengPDF.Core.Models;
using XiaoPengPDF.Pdfium;

namespace XiaoPengPDF.Services;

public class PdfDocumentService
{
    private IPdfRenderer? _renderer;
    private IPdfTextExtractor? _textExtractor;
    private IPdfOutlineProvider? _outlineProvider;
    private IPdfSearchService? _searchService;

    public IPdfDocument? CurrentDocument { get; private set; }

    public void LoadDocument(string filePath)
    {
        CloseDocument();

        var pdfiumDoc = new PdfiumDocument(filePath);
        CurrentDocument = pdfiumDoc;

        _renderer = new PdfiumRenderer();
        _textExtractor = new PdfiumTextExtractor();
        _outlineProvider = new PdfiumOutlineProvider();
        _searchService = new PdfiumSearchService();
    }

    public IPdfRenderer Renderer => _renderer ?? throw new InvalidOperationException("No document loaded");
    public IPdfTextExtractor TextExtractor => _textExtractor ?? throw new InvalidOperationException("No document loaded");
    public IPdfOutlineProvider OutlineProvider => _outlineProvider ?? throw new InvalidOperationException("No document loaded");
    public IPdfSearchService SearchService => _searchService ?? throw new InvalidOperationException("No document loaded");

    public void CloseDocument()
    {
        CurrentDocument?.Dispose();
        CurrentDocument = null;
        _renderer?.Dispose();
        _textExtractor = null;
        _outlineProvider = null;
        _searchService = null;
    }
}
