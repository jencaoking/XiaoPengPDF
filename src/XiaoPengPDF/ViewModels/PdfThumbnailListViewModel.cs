using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using XiaoPengPDF.Services;
using XiaoPengPDF.Core.Models;
using XiaoPengPDF.Infrastructure.Logging;

namespace XiaoPengPDF.ViewModels;

public partial class PdfThumbnailViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _pageNumber;

    [ObservableProperty]
    private byte[]? _thumbnail;

    [ObservableProperty]
    private bool _isSelected;
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
                thumbnail.Thumbnail = documentService.Renderer.RenderThumbnail(
                    documentService.CurrentDocument,
                    i,
                    150,
                    200);
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
