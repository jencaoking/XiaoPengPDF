using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using XiaoPengPDF.ViewModels;
using XiaoPengPDF.Infrastructure.Logging;
using XiaoPengPDF.Infrastructure.Configuration;
using XiaoPengPDF.Services;

namespace XiaoPengPDF.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        LoggingService.Info("MainWindow constructor starting...");
        InitializeComponent();
        LoggingService.Info("MainWindow InitializeComponent done");

        AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
        AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
        AddHandler(DragDrop.DropEvent, OnDrop);

        KeyDown += OnKeyDown;

        Loaded += OnLoaded;
        LoggingService.Info("MainWindow constructor completed");
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel)
        {
            _ = new RecentFilesService(AppSettings.Load());
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);

            if (e.Key == Key.O && ctrl)
            {
                vm.OpenCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.F && ctrl)
            {
                vm.ToggleSearchCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.G && ctrl)
            {
                vm.ShowGoToPageCommand.Execute(null);
                e.Handled = true;
            }
            else if ((e.Key == Key.D0 && ctrl) || (e.Key == Key.NumPad0 && ctrl))
            {
                vm.FitPageCommand.Execute(null);
                e.Handled = true;
            }
            else if ((e.Key == Key.D1 && ctrl) || (e.Key == Key.NumPad1 && ctrl))
            {
                vm.FitWidthCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                vm.CloseSearchCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                vm.PreviousPageCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                vm.NextPageCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.Home)
            {
                vm.GoToFirstPageCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.End)
            {
                vm.GoToLastPageCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.F11)
            {
                vm.FullScreenCommand.Execute(null);
                e.Handled = true;
            }
        }
    }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            e.DragEffects = DragDropEffects.Copy;
        }
    }

    private void OnDragLeave(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.None;
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files) && DataContext is MainWindowViewModel vm)
        {
            var files = e.Data.GetFiles();
            if (files != null)
            {
                var fileList = files.ToList();
                if (fileList.Count > 0)
                {
                    var path = fileList[0].Path.LocalPath;
                    if (path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        LoggingService.Info($"Opening dropped file: {path}");
                        vm.OpenFile(path);
                    }
                }
            }
        }
    }
}
