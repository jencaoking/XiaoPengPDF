using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;
using XiaoPengPDF.Services;
using XiaoPengPDF.Core.Models;
using XiaoPengPDF.Infrastructure.Logging;

namespace XiaoPengPDF.ViewModels;

public partial class PdfThumbnailViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _pageNumber;

    [ObservableProperty]
    private Bitmap? _thumbnailBitmap;

    [ObservableProperty]
    private bool _isSelected;

    public void SetThumbnailData(byte[] thumbnailData, int width, int height)
    {
        if (thumbnailData == null || thumbnailData.Length == 0)
            return;

        try
        {
            using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            IntPtr pixels = bitmap.GetPixels();
            Marshal.Copy(thumbnailData, 0, pixels, thumbnailData.Length);

            using (var image = SKImage.FromBitmap(bitmap))
            {
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    using var stream = new MemoryStream();
                    data.SaveTo(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    ThumbnailBitmap?.Dispose();
                    ThumbnailBitmap = new Bitmap(stream);
                }
            }
        }
        catch (Exception ex)
        {
            LoggingService.Error($"Failed to convert thumbnail for page {PageNumber}", ex);
        }
    }
}

public partial class PdfThumbnailListViewModel : ViewModelBase
{
    private readonly PdfDocumentService _documentService;
    private readonly System.Action<int>? _onPageSelected;

    [ObservableProperty]
    private ObservableCollection<PdfThumbnailViewModel> _thumbnails = new();

    [ObservableProperty]
    private int _selectedPage = 1;

    public PdfThumbnailListViewModel(PdfDocumentService documentService, System.Action<int>? onPageSelected = null)
    {
        _documentService = documentService;
        _onPageSelected = onPageSelected;
    }

    public void LoadDocument(PdfDocumentService documentService)
    {
        if (documentService.CurrentDocument == null) return;

        Thumbnails.Clear();
        for (int i = 0; i < documentService.CurrentDocument.PageCount; i++)
        {
            var thumbnail = new PdfThumbnailViewModel
            {
                PageNumber = i + 1
            };

            try
            {
                var thumbnailData = documentService.Renderer.RenderThumbnail(
                    documentService.CurrentDocument,
                    i,
                    150,
                    200);
                thumbnail.SetThumbnailData(thumbnailData, 150, 200);
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Failed to render thumbnail for page {i + 1}", ex);
            }

            Thumbnails.Add(thumbnail);
        }
    }

    partial void OnSelectedPageChanged(int value)
    {
        if (value >= 1 && _onPageSelected != null)
        {
            _onPageSelected(value);
        }
    }
}
