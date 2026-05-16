namespace XiaoPengPDF.Core;

public static class AppConstants
{
    public const string AppName = "XiaoPengPDF";
    public const string AppDisplayName = "小鹏PDF阅读器";
    public const string AppVersion = "1.0.0";

    public const string SettingsFileName = "settings.json";
    public const string BookmarksFileName = "bookmarks.json";
    public const string LogsDirectoryName = "logs";
    public const string RuntimesDirectoryName = "runtimes";
    public const string LogFileNameFormat = "app_{0:yyyyMMdd}.log";

    public const string DefaultTheme = "Light";
    public const string DarkTheme = "Dark";
    public const string DefaultLanguage = "en-US";
    public const double DefaultZoom = 1.0;
    public const string DefaultFitMode = "FitWidth";
    public const int DefaultMaxRecentFiles = 10;

    public const double MinZoom = 0.1;
    public const double MaxZoom = 5.0;
    public const double ZoomStep = 0.1;
    public const double DefaultRenderScale = 1.0;

    public const int DefaultAvailableWidth = 800;
    public const int DefaultAvailableHeight = 600;

    public const int ThumbnailWidth = 150;
    public const int ThumbnailHeight = 200;

    public const int PngEncodeQuality = 100;

    public const int WindowMinWidth = 800;
    public const int WindowMinHeight = 600;
    public const int WindowDefaultWidth = 1200;
    public const int WindowDefaultHeight = 800;

    public const double ToolbarHeight = 48;
    public const double SidebarWidth = 280;
    public const double StatusBarHeight = 32;

    public const string PdfFileExtension = ".pdf";
    public const string PdfFileFilterPattern = "*.pdf";
    public const string PdfFileTypeName = "PDF文件";

    public const string NativeLibraryName = "pdfium";
    public const string PdfiumDllWindows = "pdfium.dll";
    public const string PdfiumDylibMacOS = "libpdfium.dylib";
    public const string PdfiumSoLinux = "libpdfium.so";
}