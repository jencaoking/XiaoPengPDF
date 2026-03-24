using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using XiaoPengPDF.Core.Enums;
using XiaoPengPDF.Services;
using XiaoPengPDF.Core.Interfaces;
using XiaoPengPDF.Core.Models;
using XiaoPengPDF.Infrastructure.Logging;

namespace XiaoPengPDF.ViewModels;

public partial class PdfViewerViewModel : ViewModelBase
{
    private readonly PdfDocumentService _documentService;
    private PdfFitMode _fitMode = PdfFitMode.FitWidth;
    private double _availableWidth = 800;
    private double _availableHeight = 600;

    [ObservableProperty]
    private double _zoom = 1.0;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private byte[]? _currentPageImage;

    [ObservableProperty]
    private Bitmap? _currentPageImageSource;

    [ObservableProperty]
    private double _pageWidth;

    [ObservableProperty]
    private double _pageHeight;

    [ObservableProperty]
    private bool _isDocumentLoaded = false;

    [ObservableProperty]
    private string _dropHintText = "拖拽PDF文件到此处打开";

    public PdfViewerViewModel(PdfDocumentService documentService)
    {
        _documentService = documentService;
    }

    public void SetAvailableSize(double width, double height)
    {
        _availableWidth = width;
        _availableHeight = height;
        if (_fitMode != PdfFitMode.Custom)
        {
            RenderCurrentPage();
        }
    }

    public void LoadDocument(PdfDocumentService documentService)
    {
        if (documentService.CurrentDocument == null) return;
        if (documentService.CurrentDocument.PageCount == 0) return;

        IsDocumentLoaded = true;
        _fitMode = PdfFitMode.FitWidth;
        Zoom = 1.0;
        CurrentPage = 1;
        RenderCurrentPage();
    }

    public void SetZoom(double zoom)
    {
        Zoom = Math.Clamp(zoom, 0.1, 5.0);
        _fitMode = PdfFitMode.Custom;
        RenderCurrentPage();
    }

    public void SetFitMode(PdfFitMode fitMode)
    {
        _fitMode = fitMode;
        RenderCurrentPage();
    }

    public void GoToPage(int pageNumber)
    {
        if (_documentService.CurrentDocument == null) return;

        if (pageNumber >= 1 && pageNumber <= _documentService.CurrentDocument.PageCount)
        {
            CurrentPage = pageNumber;
            RenderCurrentPage();
        }
    }

    private void RenderCurrentPage()
    {
        if (_documentService.CurrentDocument == null || _documentService.CurrentDocument.PageCount == 0)
            return;

        try
        {
            var page = _documentService.CurrentDocument.GetPage(CurrentPage - 1);
            PageWidth = page.Width;
            PageHeight = page.Height;

            double scale = CalculateScale();
            int renderWidth = (int)(PageWidth * scale);
            int renderHeight = (int)(PageHeight * scale);

            CurrentPageImage = _documentService.Renderer.RenderPage(
                _documentService.CurrentDocument,
                CurrentPage - 1,
                renderWidth,
                renderHeight,
                1.0);

            if (CurrentPageImage != null)
            {
                using var bitmap = new SKBitmap(renderWidth, renderHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
                IntPtr pixels = bitmap.GetPixels();
                Marshal.Copy(CurrentPageImage, 0, pixels, CurrentPageImage.Length);

                using (var image = SKImage.FromBitmap(bitmap))
                {
                    using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                    {
                        using var stream = new MemoryStream();
                        data.SaveTo(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        CurrentPageImageSource?.Dispose();
                        CurrentPageImageSource = new Bitmap(stream);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LoggingService.Error($"Failed to render page {CurrentPage}", ex);
        }
    }

    private double CalculateScale()
    {
        if (PageWidth <= 0 || PageHeight <= 0)
            return Zoom;

        return _fitMode switch
        {
            PdfFitMode.FitWidth => _availableWidth / PageWidth,
            PdfFitMode.FitPage => Math.Min(_availableWidth / PageWidth, _availableHeight / PageHeight),
            PdfFitMode.Custom => Zoom,
            _ => Zoom
        };
    }
}
