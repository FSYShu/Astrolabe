using Microsoft.UI.Xaml;
using ShuRuk.Runtime.Contracts;

namespace Astrolabe.App.Services;

public class AppContext : IAppContext
{
    private readonly Window _window;

    public AppContext(Window window)
    {
        _window = window;
    }

    public IServiceProvider Services => Astrolabe.App.App.Services!;

    public string AppTitle => (_window as MainWindow)?.AppTitle ?? "Astrolabe";

    public string AppSubtitle => (_window as MainWindow)?.AppSubtitleText ?? "";

    public string AppVersion
    {
        get
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return version is not null ? $"{version.Major}.{version.Minor}.{version.Build}" : "0.1.0";
        }
    }

    public void ApplyTheme(string theme)
    {
        var elementTheme = theme switch
        {
            "light" => ElementTheme.Light,
            "dark" => ElementTheme.Dark,
            _ => ElementTheme.Default
        };

        if (_window.Content is FrameworkElement root)
        {
            root.RequestedTheme = elementTheme;
        }
    }

    public void Restart()
    {
        var exePath = Environment.ProcessPath;
        if (exePath is not null)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(exePath)
            {
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(exePath) ?? System.AppContext.BaseDirectory
            });
        }
        Environment.Exit(0);
    }

    public void CloseMainWindow()
    {
        _window.Close();
    }
}