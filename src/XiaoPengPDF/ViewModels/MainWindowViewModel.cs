using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Interactivity;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XiaoPengPDF.Services;
using XiaoPengPDF.Infrastructure.Configuration;
using XiaoPengPDF.Infrastructure.Logging;
using XiaoPengPDF.Core.Models;
using XiaoPengPDF.Core.Enums;

namespace XiaoPengPDF.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly PdfDocumentService _documentService;
    private readonly SettingsService _settingsService;
    private readonly RecentFilesService _recentFilesService;
    private readonly BookmarkService _bookmarkService;
    private Window? _window;
    private List<PdfSearchResult> _searchResults = new();
    private int _currentSearchIndex = -1;

    [ObservableProperty]
    private string _title = "小鹏PDF阅读器";

    [ObservableProperty]
    private string _statusText = "就绪";

    [ObservableProperty]
    private string _searchResultInfo = "";

    [ObservableProperty]
    private string _zoomInfo = "100%";

    [ObservableProperty]
    private string _zoomPercent = "100%";

    [ObservableProperty]
    private bool _isSidebarVisible = true;

    [ObservableProperty]
    private bool _isSearchPanelVisible = false;

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private object? _currentSidebarContent;

    [ObservableProperty]
    private object? _pdfViewerContent;

    [ObservableProperty]
    private PdfViewerViewModel? _pdfViewerViewModel;

    [ObservableProperty]
    private PdfThumbnailListViewModel? _thumbnailListViewModel;

    [ObservableProperty]
    private PdfOutlineViewModel? _outlineViewModel;

    [ObservableProperty]
    private BookmarkPanelViewModel? _bookmarkPanelViewModel;

    [ObservableProperty]
    private double _currentZoom = 1.0;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 0;

    [ObservableProperty]
    private string _currentPageInput = "1";

    [ObservableProperty]
    private PdfDocument? _currentDocument;

    public MainWindowViewModel()
    {
        _settingsService = new SettingsService();
        _documentService = new PdfDocumentService();
        _recentFilesService = new RecentFilesService(_settingsService.Settings);
        _bookmarkService = new BookmarkService();

        PdfViewerViewModel = new PdfViewerViewModel(_documentService);
        ThumbnailListViewModel = new PdfThumbnailListViewModel(_documentService, GoToPage);
        OutlineViewModel = new PdfOutlineViewModel(_documentService);
        BookmarkPanelViewModel = new BookmarkPanelViewModel(
            _bookmarkService,
            () => _documentService.CurrentDocument?.FilePath ?? "",
            GoToPage);

        PdfViewerContent = PdfViewerViewModel;
    }

    partial void OnCurrentPageInputChanged(string value)
    {
        if (TotalPages <= 0) return;
        if (int.TryParse(value, out int page) && page >= 1 && page <= TotalPages)
        {
            GoToPage(page);
        }
    }

    public void SetWindow(Window window)
    {
        _window = window;
    }

    [RelayCommand]
    private async Task Open()
    {
        if (_window == null) return;

        var files = await _window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "打开PDF文件",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("PDF文件") { Patterns = new[] { "*.pdf" } }
            }
        });

        if (files.Count > 0)
        {
            OpenFile(files[0].Path.LocalPath);
        }
    }

    public void OpenFile(string filePath)
    {
        try
        {
            LoggingService.Info($"Opening file: {filePath}");
            _documentService.LoadDocument(filePath);

            CurrentDocument = new PdfDocument
            {
                FilePath = filePath,
                FileName = System.IO.Path.GetFileName(filePath),
                PageCount = _documentService.CurrentDocument!.PageCount
            };

            TotalPages = CurrentDocument.PageCount;
            CurrentPage = 1;
            CurrentPageInput = "1";
            Title = $"小鹏PDF阅读器 - {CurrentDocument.FileName}";

            PdfViewerViewModel?.LoadDocument(_documentService);
            ThumbnailListViewModel?.LoadDocument(_documentService);
            OutlineViewModel?.LoadDocument(_documentService);
            BookmarkPanelViewModel?.LoadBookmarks(filePath, CurrentPage);

            _recentFilesService.AddRecentFile(filePath);
            StatusText = $"已加载: {filePath}";
        }
        catch (Exception ex)
        {
            LoggingService.Error($"Failed to open file: {filePath}", ex);
            StatusText = $"打开失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            CurrentPageInput = CurrentPage.ToString();
            PdfViewerViewModel?.GoToPage(CurrentPage);
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            CurrentPageInput = CurrentPage.ToString();
            PdfViewerViewModel?.GoToPage(CurrentPage);
        }
    }

    private void GoToPage(int pageNumber)
    {
        if (pageNumber >= 1 && pageNumber <= TotalPages)
        {
            CurrentPage = pageNumber;
            PdfViewerViewModel?.GoToPage(CurrentPage);
        }
    }

    [RelayCommand]
    private void GoToFirstPage()
    {
        if (TotalPages > 0)
        {
            CurrentPage = 1;
            CurrentPageInput = "1";
            PdfViewerViewModel?.GoToPage(CurrentPage);
        }
    }

    [RelayCommand]
    private void GoToLastPage()
    {
        if (TotalPages > 0)
        {
            CurrentPage = TotalPages;
            CurrentPageInput = TotalPages.ToString();
            PdfViewerViewModel?.GoToPage(CurrentPage);
        }
    }

    [RelayCommand]
    private void ShowGoToPage()
    {
        IsSearchPanelVisible = false;
    }

    [RelayCommand]
    private void Close()
    {
        _documentService.CloseDocument();
        CurrentDocument = null;
        TotalPages = 0;
        CurrentPage = 1;
        CurrentPageInput = "1";
        Title = "小鹏PDF阅读器";
        StatusText = "就绪";
        _searchResults.Clear();
        _currentSearchIndex = -1;
    }

    [RelayCommand]
    private void Exit()
    {
        _window?.Close();
    }

    [RelayCommand]
    private void ZoomIn()
    {
        if (CurrentZoom < 5.0)
        {
            CurrentZoom += 0.1;
            UpdateZoom();
        }
    }

    [RelayCommand]
    private void ZoomOut()
    {
        if (CurrentZoom > 0.1)
        {
            CurrentZoom -= 0.1;
            UpdateZoom();
        }
    }

    private void UpdateZoom()
    {
        ZoomInfo = $"{(int)(CurrentZoom * 100)}%";
        ZoomPercent = ZoomInfo;
        PdfViewerViewModel?.SetZoom(CurrentZoom);
    }

    [RelayCommand]
    private void FitWidth()
    {
        PdfViewerViewModel?.SetFitMode(PdfFitMode.FitWidth);
        ZoomInfo = "适应宽度";
    }

    [RelayCommand]
    private void FitPage()
    {
        PdfViewerViewModel?.SetFitMode(PdfFitMode.FitPage);
        ZoomInfo = "适应页面";
    }

    [RelayCommand]
    private void FullScreen()
    {
        if (_window != null)
        {
            _window.WindowState = _window.WindowState == WindowState.FullScreen
                ? WindowState.Normal
                : WindowState.FullScreen;
        }
    }

    [RelayCommand]
    private void ToggleSearch()
    {
        IsSearchPanelVisible = !IsSearchPanelVisible;
        if (!IsSearchPanelVisible)
        {
            SearchText = "";
            _searchResults.Clear();
            _currentSearchIndex = -1;
            SearchResultInfo = "";
        }
    }

    [RelayCommand]
    private void CloseSearch()
    {
        IsSearchPanelVisible = false;
        SearchText = "";
        _searchResults.Clear();
        _currentSearchIndex = -1;
        SearchResultInfo = "";
    }

    [RelayCommand]
    private void Search()
    {
        if (string.IsNullOrWhiteSpace(SearchText) || _documentService.CurrentDocument == null)
        {
            SearchResultInfo = "请输入搜索内容";
            return;
        }

        try
        {
            StatusText = $"正在搜索: {SearchText}";
            _searchResults = _documentService.SearchService.Search(_documentService.CurrentDocument, SearchText);
            _currentSearchIndex = _searchResults.Count > 0 ? 0 : -1;

            if (_searchResults.Count > 0)
            {
                var firstResult = _searchResults[0];
                GoToPage(firstResult.PageNumber + 1);
                SearchResultInfo = $"找到 {_searchResults.Count} 个结果 (1/{_searchResults.Count})";
                StatusText = $"找到 {firstResult.PageNumber + 1} 页";
            }
            else
            {
                SearchResultInfo = "未找到匹配项";
                StatusText = "搜索完成";
            }
        }
        catch (Exception ex)
        {
            LoggingService.Error($"Search failed: {SearchText}", ex);
            SearchResultInfo = "搜索失败";
            StatusText = $"搜索失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private void SearchPrevious()
    {
        if (_searchResults.Count == 0) return;

        _currentSearchIndex--;
        if (_currentSearchIndex < 0)
            _currentSearchIndex = _searchResults.Count - 1;

        NavigateToSearchResult();
    }

    [RelayCommand]
    private void SearchNext()
    {
        if (_searchResults.Count == 0) return;

        _currentSearchIndex++;
        if (_currentSearchIndex >= _searchResults.Count)
            _currentSearchIndex = 0;

        NavigateToSearchResult();
    }

    private void NavigateToSearchResult()
    {
        if (_currentSearchIndex < 0 || _currentSearchIndex >= _searchResults.Count) return;

        var result = _searchResults[_currentSearchIndex];
        GoToPage(result.PageNumber + 1);
        SearchResultInfo = $"找到 {_searchResults.Count} 个结果 ({_currentSearchIndex + 1}/{_searchResults.Count})";
        StatusText = $"跳转到第 {result.PageNumber + 1} 页";
    }

    [RelayCommand]
    private void ShowThumbnails()
    {
        CurrentSidebarContent = ThumbnailListViewModel;
        IsSidebarVisible = true;
    }

    [RelayCommand]
    private void ShowOutline()
    {
        CurrentSidebarContent = OutlineViewModel;
        IsSidebarVisible = true;
    }

    [RelayCommand]
    private void ShowBookmarks()
    {
        CurrentSidebarContent = BookmarkPanelViewModel;
        IsSidebarVisible = true;
    }

    [RelayCommand]
    private void About()
    {
        StatusText = "小鹏PDF阅读器 v1.0.0";
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        App.ToggleTheme();
        StatusText = App.IsDarkMode ? "已切换到暗色模式" : "已切换到浅色模式";
    }

    [RelayCommand]
    private void Print()
    {
        if (_documentService.CurrentDocument == null)
        {
            StatusText = "请先打开PDF文件";
            return;
        }

        try
        {
            StatusText = "正在准备打印...";
            var printDialog = new Window
            {
                Title = "打印",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var stackPanel = new StackPanel { Margin = new Avalonia.Thickness(20), Spacing = 10 };
            stackPanel.Children.Add(new TextBlock { Text = "打印功能开发中...", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center });
            stackPanel.Children.Add(new TextBlock { Text = $"文件名: {CurrentDocument?.FileName}", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center });
            stackPanel.Children.Add(new TextBlock { Text = $"页数: {TotalPages}", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center });

            var closeButton = new Button { Content = "关闭", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
            closeButton.Click += (s, e) => printDialog.Close();
            stackPanel.Children.Add(closeButton);

            printDialog.Content = stackPanel;
            printDialog.Show();

            StatusText = "打印对话框已打开";
        }
        catch (Exception ex)
        {
            LoggingService.Error("Print failed", ex);
            StatusText = $"打印失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CopyCurrentPageText()
    {
        if (_documentService.CurrentDocument == null)
        {
            StatusText = "请先打开PDF文件";
            return;
        }

        try
        {
            var text = _documentService.TextExtractor.ExtractText(_documentService.CurrentDocument, CurrentPage - 1);
            if (!string.IsNullOrEmpty(text))
            {
                var topLevel = TopLevel.GetTopLevel(_window);
                if (topLevel != null && topLevel.Clipboard != null)
                {
                    await topLevel.Clipboard.SetTextAsync(text);
                    StatusText = "已复制当前页文本到剪贴板";
                }
                else
                {
                    StatusText = "无法访问剪贴板";
                }
            }
            else
            {
                StatusText = "当前页没有可复制的文本";
            }
        }
        catch (Exception ex)
        {
            LoggingService.Error("Copy text failed", ex);
            StatusText = $"复制失败: {ex.Message}";
        }
    }
}
