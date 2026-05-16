using System.Reflection;
using System.Runtime.InteropServices;
using XiaoPengPDF.Core;

namespace XiaoPengPDF.Pdfium;

public static class PdfiumNativeLoader
{
    private static bool _isInitialized = false;
    private static readonly Lock _lock = new();
    private static IntPtr _pdfiumHandle = IntPtr.Zero;

    public static void Initialize()
    {
        if (_isInitialized) return;

        lock (_lock)
        {
            if (_isInitialized) return;

            string dllName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? AppConstants.PdfiumDllWindows
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? AppConstants.PdfiumDylibMacOS
                    : AppConstants.PdfiumSoLinux;

            string[] searchPaths = GetSearchPaths();

            foreach (var path in searchPaths)
            {
                if (File.Exists(path))
                {
                    if (NativeLibrary.TryLoad(path, out _pdfiumHandle))
                    {
                        NativeLibrary.SetDllImportResolver(typeof(PdfiumNativeLoader).Assembly, DllImportResolver);
                        _isInitialized = true;
                        return;
                    }
                }
            }

            if (NativeLibrary.TryLoad(dllName, out _pdfiumHandle))
            {
                NativeLibrary.SetDllImportResolver(typeof(PdfiumNativeLoader).Assembly, DllImportResolver);
                _isInitialized = true;
            }
        }
    }

    private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName == AppConstants.NativeLibraryName || libraryName == AppConstants.PdfiumDllWindows || libraryName == AppConstants.PdfiumSoLinux || libraryName == AppConstants.PdfiumDylibMacOS)
        {
            return _pdfiumHandle;
        }
        return IntPtr.Zero;
    }

    private static string[] GetSearchPaths()
    {
        var paths = new List<string>();
        string basePath = AppDomain.CurrentDomain.BaseDirectory;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string arch = RuntimeInformation.ProcessArchitecture == Architecture.X64 ? "x64" : "x86";
            paths.Add(Path.Combine(basePath, arch, AppConstants.PdfiumDllWindows));
            paths.Add(Path.Combine(basePath, AppConstants.RuntimesDirectoryName, arch, "native", AppConstants.PdfiumDllWindows));
            paths.Add(Path.Combine(basePath, AppConstants.RuntimesDirectoryName, arch, AppConstants.PdfiumDllWindows));
            paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppConstants.AppName, AppConstants.RuntimesDirectoryName, arch, AppConstants.PdfiumDllWindows));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            paths.Add(Path.Combine(basePath, AppConstants.RuntimesDirectoryName, "linux-x64", "native", AppConstants.PdfiumSoLinux));
            paths.Add(Path.Combine(basePath, AppConstants.RuntimesDirectoryName, "linux-x64", AppConstants.PdfiumSoLinux));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            paths.Add(Path.Combine(basePath, AppConstants.RuntimesDirectoryName, "osx-x64", "native", AppConstants.PdfiumDylibMacOS));
            paths.Add(Path.Combine(basePath, AppConstants.RuntimesDirectoryName, "osx-x64", AppConstants.PdfiumDylibMacOS));
        }

        return [.. paths];
    }

    public static IntPtr GetHandle() => _pdfiumHandle;
}
