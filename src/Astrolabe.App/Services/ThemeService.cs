using Astrolabe.App.Constants;
using Microsoft.UI.Xaml;
using ShuRuk.Runtime.Contracts;

namespace Astrolabe.App.Services;

public class ThemeService : IThemeService
{
    private string _currentTheme = AppConstants.Themes.System;

    public string CurrentTheme => _currentTheme;

    public event EventHandler<string>? ThemeChanged;

    public List<(string Code, string DisplayName)> AvailableThemes { get; } =
    [
        (AppConstants.Themes.System, "Follow system"),
        (AppConstants.Themes.Light, "Light"),
        (AppConstants.Themes.Dark, "Dark")
    ];

    public Task SetThemeAsync(string theme)
    {
        if (_currentTheme == theme) return Task.CompletedTask;
        _currentTheme = theme;
        ThemeChanged?.Invoke(this, theme);
        return Task.CompletedTask;
    }

    public ElementTheme ToElementTheme()
    {
        return _currentTheme switch
        {
            AppConstants.Themes.Light => ElementTheme.Light,
            AppConstants.Themes.Dark => ElementTheme.Dark,
            _ => ElementTheme.Default
        };
    }
}