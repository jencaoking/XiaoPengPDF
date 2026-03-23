using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using XiaoPengPDF.Views;
using XiaoPengPDF.ViewModels;
using XiaoPengPDF.Infrastructure.Logging;
using XiaoPengPDF.Infrastructure.Configuration;
using XiaoPengPDF.Pdfium;

namespace XiaoPengPDF;

public partial class App : Application
{
    public static bool IsDarkMode { get; private set; } = false;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        LoggingService.Initialize();
        LoggingService.Info("XiaoPengPDF starting...");

        PdfiumNativeLoader.Initialize();
        LoggingService.Info("PDFium native library initialized");

        var settings = AppSettings.Load();
        ApplyTheme(settings.Theme == "Dark");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static void ApplyTheme(bool isDark)
    {
        IsDarkMode = isDark;
        if (Current != null)
        {
            Current.RequestedThemeVariant = isDark
                ? Avalonia.Styling.ThemeVariant.Dark
                : Avalonia.Styling.ThemeVariant.Light;
        }
    }

    public static void ToggleTheme()
    {
        ApplyTheme(!IsDarkMode);
        var settings = AppSettings.Load();
        settings.Theme = IsDarkMode ? "Dark" : "Light";
        settings.Save();
    }
}
