using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XiaoPengPDF.Core.Models;
using XiaoPengPDF.Services;

namespace XiaoPengPDF.ViewModels;

public partial class BookmarkItemViewModel : ViewModelBase
{
    private readonly BookmarkService _bookmarkService;
    private readonly PdfBookmark _bookmark;
    private readonly System.Action<int> _onNavigate;

    [ObservableProperty]
    private string _title = "";

    [ObservableProperty]
    private string _pageInfo = "";

    public BookmarkItemViewModel(PdfBookmark bookmark, BookmarkService bookmarkService, System.Action<int> onNavigate)
    {
        _bookmark = bookmark;
        _bookmarkService = bookmarkService;
        _onNavigate = onNavigate;
        Title = bookmark.Title;
        PageInfo = $"第 {bookmark.PageNumber} 页";
    }

    [RelayCommand]
    private void Navigate()
    {
        _onNavigate?.Invoke(_bookmark.PageNumber);
    }

    [RelayCommand]
    private async Task Remove()
    {
        await _bookmarkService.RemoveBookmarkAsync(_bookmark.Id);
    }
}

public partial class BookmarkPanelViewModel : ViewModelBase
{
    private readonly BookmarkService _bookmarkService;
    private readonly System.Func<string> _getCurrentFilePath;
    private readonly System.Action<int> _onNavigateToPage;
    private string _currentFilePath = "";

    [ObservableProperty]
    private ObservableCollection<BookmarkItemViewModel> _bookmarks = new();

    [ObservableProperty]
    private bool _hasBookmarks = false;

    public BookmarkPanelViewModel(BookmarkService bookmarkService, System.Func<string> getCurrentFilePath, System.Action<int> onNavigateToPage)
    {
        _bookmarkService = bookmarkService;
        _getCurrentFilePath = getCurrentFilePath;
        _onNavigateToPage = onNavigateToPage;
    }

    public void LoadBookmarks(string filePath, int currentPage)
    {
        _currentFilePath = filePath;
        Bookmarks.Clear();

        if (string.IsNullOrEmpty(filePath)) return;

        var bookmarks = _bookmarkService.GetBookmarksAsync(filePath).GetAwaiter().GetResult();

        foreach (var bookmark in bookmarks)
        {
            Bookmarks.Add(new BookmarkItemViewModel(bookmark, _bookmarkService, _onNavigateToPage));
        }

        HasBookmarks = Bookmarks.Count > 0;
    }

    [RelayCommand]
    private async Task AddBookmark()
    {
        if (string.IsNullOrEmpty(_currentFilePath)) return;

        var bookmark = new PdfBookmark
        {
            FilePath = _currentFilePath,
            PageNumber = 1,
            Title = $"书签 {Bookmarks.Count + 1}"
        };

        await _bookmarkService.AddBookmarkAsync(bookmark);
        Bookmarks.Add(new BookmarkItemViewModel(bookmark, _bookmarkService, _onNavigateToPage));
        HasBookmarks = true;
    }

    public void Refresh()
    {
        if (!string.IsNullOrEmpty(_currentFilePath))
        {
            LoadBookmarks(_currentFilePath, 1);
        }
    }
}