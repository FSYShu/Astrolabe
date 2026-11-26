using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
using WinRT.Interop;
using Astrolabe.App.Constants;

using ShuRuk.Runtime.Contracts;
using ShuRuk.Runtime.Core;

namespace Astrolabe.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; set; } = null!;
    private Window? _mainWindow;

    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                System.Diagnostics.Debug.WriteLine($"[AppDomain.UnhandledException] {ex}");
        };

        this.UnhandledException += OnUnhandledException;

        InitializeComponent();
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[UnhandledException] {e.Exception}");
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        RegisterServices();

        var window = new MainWindow();
        window.Activate();
        _mainWindow = window;
        LoadAppIcon(window);
        RegisterRuntimeServices(window);
    }

    private void RegisterServices()
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ShuRuk");
        Directory.CreateDirectory(appDataPath);

        var languageService = new Services.LanguageService();
        languageService.LoadAsync().Wait();
        var themeService = new Services.ThemeService();
        var testModeService = new Services.TestModeService();

        var services = new ServiceCollection();
        services.AddSingleton<ILanguageService>(languageService);
        services.AddSingleton<IThemeService>(themeService);
        services.AddSingleton<ITestModeService>(testModeService);

        Services = services.BuildServiceProvider();

    }

    private void RegisterRuntimeServices(Window window)
    {
        RuntimeServices.LanguageService = Services.GetRequiredService<ILanguageService>();
        RuntimeServices.ThemeService = Services.GetRequiredService<IThemeService>();
        RuntimeServices.TestModeService = Services.GetRequiredService<ITestModeService>();
        RuntimeServices.AppContext = new Services.AppContext(window);
    }

    private void LoadAppIcon(Window window)
    {
        try
        {
            window.AppWindow.SetIcon(AppConstants.AppIconPath);
            SetWindowAppUserModelProperties(window, AppConstants.AppIconPath);
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"LoadAppIcon failed: {ex}"); }
    }

    private void SetWindowAppUserModelProperties(Window window, string icoRelativePath)
    {
        try
        {
            var hWnd = WindowNative.GetWindowHandle(window);
            var icoFullPath = Path.GetFullPath(icoRelativePath);

            var riid = typeof(IPropertyStore).GUID;
            var hr = SHGetPropertyStoreForWindow(hWnd, ref riid, out var psObj);
            if (hr != 0 || psObj is not IPropertyStore ps) return;

            var idKey = new PROPERTYKEY
            {
                fmtid = new Guid(AppConstants.AppUserModelFmtid),
                pid = AppConstants.AppUserModelIdPid
            };
            var idProp = PROPVARIANT.FromString(AppConstants.AppUserModelIdPackaged);

            var iconKey = new PROPERTYKEY
            {
                fmtid = new Guid(AppConstants.AppUserModelFmtid),
                pid = AppConstants.AppUserModelIconPid
            };
            var iconProp = PROPVARIANT.FromString(icoFullPath);

            try
            {
                ps.SetValue(ref idKey, idProp);
                ps.SetValue(ref iconKey, iconProp);
                ps.Commit();
            }
            finally
            {
                idProp.Dispose();
                iconProp.Dispose();
                Marshal.ReleaseComObject(ps);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SetWindowAppUserModelProperties failed: {ex}");
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PROPERTYKEY
    {
        public Guid fmtid;
        public int pid;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PROPVARIANT : IDisposable
    {
        public ushort vt;
        public ushort wReserved1;
        public ushort wReserved2;
        public ushort wReserved3;
        public IntPtr pwszVal;

        public static PROPVARIANT FromString(string value)
        {
            var pv = new PROPVARIANT();
            pv.vt = (ushort)VarEnum.VT_LPWSTR;
            pv.pwszVal = Marshal.StringToCoTaskMemUni(value);
            return pv;
        }

        public void Dispose()
        {
            if (vt == (ushort)VarEnum.VT_LPWSTR && pwszVal != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(pwszVal);
                pwszVal = IntPtr.Zero;
            }
        }
    }

    [ComImport]
    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPropertyStore
    {
        int GetCount(out uint cProps);
        int GetAt(uint iProp, out PROPERTYKEY pkey);
        int GetValue(ref PROPERTYKEY key, out PROPVARIANT pv);
        int SetValue(ref PROPERTYKEY key, PROPVARIANT pv);
        int Commit();
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
    private static extern int SHGetPropertyStoreForWindow(
        IntPtr hwnd, ref Guid riid,
        [Out, MarshalAs(UnmanagedType.IUnknown)] out object? ppv);
}