using System.Reflection;
using System.Runtime.InteropServices;

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
                ? "pdfium.dll"
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? "libpdfium.dylib"
                    : "libpdfium.so";

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
        if (libraryName == "pdfium" || libraryName == "pdfium.dll" || libraryName == "libpdfium.so" || libraryName == "libpdfium.dylib")
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
            paths.Add(Path.Combine(basePath, arch, "pdfium.dll"));
            paths.Add(Path.Combine(basePath, "runtimes", arch, "native", "pdfium.dll"));
            paths.Add(Path.Combine(basePath, "runtimes", arch, "pdfium.dll"));
            paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XiaoPengPDF", "runtimes", arch, "pdfium.dll"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            paths.Add(Path.Combine(basePath, "runtimes", "linux-x64", "native", "libpdfium.so"));
            paths.Add(Path.Combine(basePath, "runtimes", "linux-x64", "libpdfium.so"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            paths.Add(Path.Combine(basePath, "runtimes", "osx-x64", "native", "libpdfium.dylib"));
            paths.Add(Path.Combine(basePath, "runtimes", "osx-x64", "libpdfium.dylib"));
        }

        return [.. paths];
    }

    public static IntPtr GetHandle() => _pdfiumHandle;
}
