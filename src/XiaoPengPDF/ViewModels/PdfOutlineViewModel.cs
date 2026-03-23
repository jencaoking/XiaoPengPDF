using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using XiaoPengPDF.Services;
using XiaoPengPDF.Core.Models;

namespace XiaoPengPDF.ViewModels;

public partial class OutlineItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "";

    [ObservableProperty]
    private int _pageNumber;

    [ObservableProperty]
    private double _yPosition;

    [ObservableProperty]
    private ObservableCollection<OutlineItemViewModel> _children = new();

    [ObservableProperty]
    private bool _isExpanded = true;
}

public partial class PdfOutlineViewModel : ViewModelBase
{
    private readonly PdfDocumentService _documentService;

    [ObservableProperty]
    private ObservableCollection<OutlineItemViewModel> _outlineItems = new();

    [ObservableProperty]
    private bool _hasOutline = false;

    public PdfOutlineViewModel(PdfDocumentService documentService)
    {
        _documentService = documentService;
    }

    public void LoadDocument(PdfDocumentService documentService)
    {
        if (documentService.CurrentDocument == null) return;

        OutlineItems.Clear();

        try
        {
            var outline = documentService.OutlineProvider.GetOutline(documentService.CurrentDocument);
            HasOutline = outline.Count > 0;

            foreach (var item in outline)
            {
                OutlineItems.Add(ConvertToOutlineItem(item));
            }
        }
        catch
        {
            HasOutline = false;
        }
    }

    private OutlineItemViewModel ConvertToOutlineItem(PdfOutline item)
    {
        var vm = new OutlineItemViewModel
        {
            Title = item.Title,
            PageNumber = item.PageNumber,
            YPosition = item.YPosition
        };

        foreach (var child in item.Children)
        {
            vm.Children.Add(ConvertToOutlineItem(child));
        }

        return vm;
    }
}
