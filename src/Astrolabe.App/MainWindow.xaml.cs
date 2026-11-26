using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Astrolabe.App.Constants;

using Astrolabe.App.Models;
using ShuRuk.Runtime.Contracts;
using ShuRuk.Runtime.Core;
using ShuRuk.Runtime.Helpers;
using ShuRuk.Runtime.Views;
using RuntimeLocalizationHelper = ShuRuk.Runtime.Helpers.LocalizationHelper;

using System.Runtime.InteropServices;
using WinRT.Interop;

using Astrolabe.App.Services;

namespace Astrolabe.App;

public sealed partial class MainWindow : Window
{
    public string AppTitle => AppTitleBar.Title;
    public string AppSubtitleText => AppTitleBar.Subtitle;

    private string? _lastSidebarTag = null;
    private readonly Stack<string> _history = new();

    public MainWindow()
    {
        Title = "Astrolabe";
        var appWindow = AppWindow;

        try
        {
            SystemBackdrop = new MicaBackdrop
            {
                Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base
            };
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"MicaBackdrop not supported: {ex}"); }

        ExtendsContentIntoTitleBar = true;

        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            var titleBar = appWindow.TitleBar;
            titleBar.ExtendsContentIntoTitleBar = true;
            titleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            titleBar.BackgroundColor = Colors.Transparent;
            titleBar.InactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsMaximizable = true;
            presenter.IsMinimizable = true;
            presenter.SetBorderAndTitleBar(true, true);
        }

        InitializeComponent();
        SetTitleBar(AppTitleBar);

        AppTitleBar.Loaded += AppTitleBar_Loaded;
        AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;

        var languageService = App.Services.GetRequiredService<ILanguageService>();
        languageService.LanguageChanged += OnLanguageChanged;

        var themeService = App.Services.GetRequiredService<IThemeService>();
        var initialTheme = themeService.ToElementTheme();
        if (Content is FrameworkElement root)
        {
            root.RequestedTheme = initialTheme;
            root.ActualThemeChanged += (_, _) =>
            {
                var isDark = root.ActualTheme == ElementTheme.Dark;
                ApplyTitleBarDarkMode(isDark);
            };
        }

        RefreshNavItems();

        ContentFrame.NavigationFailed += ContentFrame_NavigationFailed;

        _suppressNavigation = true;
        if (NavView.MenuItems.Count > 0)
            NavView.SelectedItem = NavView.MenuItems[0];
        _suppressNavigation = false;
        NavigateTo("home", pushHistory: false);
        RuntimeLocalizationHelper.App.ApplyUidResources(NavView);

        NavView.Loaded += NavView_Loaded;

        DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () =>
        {
            if (_sizeInitialized) return;
            InitializeWindowSize();
        });

        Activated += MainWindow_Activated;
        SizeChanged += MainWindow_SizeChanged;
    }

    private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
    {
        if (Content is not FrameworkElement root) return;
        root.InvalidateMeasure();
        root.InvalidateArrange();
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshNavBuiltInItems();
    }

    private void OnLanguageChanged()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            RefreshNavItems();
            RefreshNavBuiltInItems();
            RuntimeLocalizationHelper.App.ApplyUidResources(NavView);
        });
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"ContentFrame_NavigationFailed: {e.Exception}");
        e.Handled = true;
    }

    private bool _sizeInitialized;

    private void InitializeWindowSize()
    {
        _sizeInitialized = true;

        var appWindow = AppWindow;
        var hWnd = WindowNative.GetWindowHandle(this);
        var dpi = GetDpiForWindow(hWnd);
        var scale = dpi / AppConstants.DpiBase;

        var width = (int)(AppConstants.DefaultWindowWidth * scale);
        var height = (int)(AppConstants.DefaultWindowHeight * scale);

        SetWindowMinSize(AppConstants.MinWindowWidth, AppConstants.MinWindowHeight);

        var screenRect = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary).WorkArea;
        var x = (screenRect.Width - width) / 2;
        var y = (screenRect.Height - height) / 2;

        appWindow.MoveAndResize(new Windows.Graphics.RectInt32(x, y, width, height));
    }

    private bool _titleBarThemeApplied;

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (!_sizeInitialized)
        {
            InitializeWindowSize();
        }

        if (!_titleBarThemeApplied)
        {
            _titleBarThemeApplied = true;
            if (Content is FrameworkElement root)
            {
                var isDark = root.ActualTheme == ElementTheme.Dark;
                ApplyTitleBarDarkMode(isDark);
            }
        }
    }

    public void ApplyTheme(ElementTheme theme)
    {
        if (Content is not FrameworkElement root) return;
        root.RequestedTheme = theme;
        DispatcherQueue.TryEnqueue(() =>
        {
            var isDark = root.ActualTheme == ElementTheme.Dark;
            ApplyTitleBarDarkMode(isDark);
        });
    }

    private static readonly bool _titleBarCustomizationSupported = AppWindowTitleBar.IsCustomizationSupported();

    private void ApplyTitleBarDarkMode(bool isDark)
    {
        var hWnd = WindowNative.GetWindowHandle(this);
        var value = isDark ? 1 : 0;
        DwmSetWindowAttribute(hWnd, AppConstants.DwmwaUseImmersiveDarkMode20, ref value, sizeof(int));
        DwmSetWindowAttribute(hWnd, AppConstants.DwmwaUseImmersiveDarkMode19, ref value, sizeof(int));

        if (_titleBarCustomizationSupported)
        {
            var titleBar = AppWindow.TitleBar;
            var fgColor = isDark ? Colors.White : Colors.Black;
            var hoverBg = isDark
                ? ColorHelper.FromArgb(AppConstants.TitleBarHoverBgAlpha, 255, 255, 255)
                : ColorHelper.FromArgb(AppConstants.TitleBarHoverBgAlpha, 0, 0, 0);
            var pressedBg = isDark
                ? ColorHelper.FromArgb(AppConstants.TitleBarPressedBgAlpha, 255, 255, 255)
                : ColorHelper.FromArgb(AppConstants.TitleBarPressedBgAlpha, 0, 0, 0);

            titleBar.ButtonForegroundColor = fgColor;
            titleBar.ButtonHoverForegroundColor = fgColor;
            titleBar.ButtonHoverBackgroundColor = hoverBg;
            titleBar.ButtonPressedForegroundColor = fgColor;
            titleBar.ButtonPressedBackgroundColor = pressedBg;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveForegroundColor =
                ColorHelper.FromArgb(AppConstants.TitleBarInactiveFgAlpha, fgColor.R, fgColor.G, fgColor.B);
        }
    }

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hWnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

    private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(ApplyTitleBarTemplateOverrides);
        AppTitleBar.LayoutUpdated += AppTitleBar_LayoutUpdated;
    }

    private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        EnqueueTitleBarFix();
    }

    private void AppTitleBar_LayoutUpdated(object? sender, object e)
    {
        AppTitleBar.LayoutUpdated -= AppTitleBar_LayoutUpdated;
        try { ApplyTitleBarTemplateOverrides(); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"AppTitleBar_LayoutUpdated: {ex}"); }
    }

    private void EnqueueTitleBarFix()
    {
        DispatcherQueue.TryEnqueue(
            Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
            ApplyTitleBarTemplateOverrides);
    }

    private void ApplyTitleBarTemplateOverrides()
    {
        try
        {
            const double buttonSize = AppConstants.TitleBarButtonSize;

            foreach (var button in UiTreeHelper.EnumerateVisualChildren<Button>(AppTitleBar))
            {
                if (button.Name != "PART_BackButton" && button.Name != "PART_PaneToggleButton")
                    continue;

                button.Width = buttonSize;
                button.Height = buttonSize;
                button.MinWidth = buttonSize;
                button.MinHeight = buttonSize;
                button.HorizontalAlignment = HorizontalAlignment.Center;
                button.VerticalAlignment = VerticalAlignment.Center;
                button.Padding = new Thickness(0);

                if (button.Name == "PART_BackButton" && !button.IsEnabled)
                {
                    foreach (var grid in UiTreeHelper.EnumerateVisualChildren<Grid>(button))
                    {
                        if (grid.Name == "RootGrid")
                            grid.Background = new SolidColorBrush(Colors.Transparent);
                    }
                }

                foreach (var border in UiTreeHelper.EnumerateVisualChildren<Border>(button))
                {
                    border.ClearValue(FrameworkElement.WidthProperty);
                    border.ClearValue(FrameworkElement.HeightProperty);
                    border.ClearValue(FrameworkElement.MinWidthProperty);
                    border.ClearValue(FrameworkElement.MinHeightProperty);
                }
            }

            var title = AppTitleBar.Title;
            foreach (var tb in UiTreeHelper.EnumerateVisualChildren<TextBlock>(AppTitleBar))
            {
                if (tb.Text == title)
                    tb.ClearValue(FrameworkElement.MinWidthProperty);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ApplyTitleBarTemplateOverrides error: {ex}");
        }
    }

    private void AppTitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        NavView.IsPaneOpen = !NavView.IsPaneOpen;
        RefreshNavBuiltInItems();
    }

    private void AppTitleBar_BackRequested(TitleBar sender, object args)
    {
        if (_history.Count == 0) return;
        var previousTag = _history.Pop();
        NavigateTo(previousTag, pushHistory: false);
    }

    private bool _suppressNavigation;

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (_suppressNavigation) return;

        if (args.IsSettingsInvoked)
        {
            NavigateTo("settings");
            return;
        }

        var tag = args.InvokedItemContainer?.Tag?.ToString();

        if (tag is not null)
            NavigateTo(tag);
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (_suppressNavigation) return;

        if (args.IsSettingsSelected)
        {
            NavigateTo("settings");
            return;
        }

        var tag = (args.SelectedItemContainer as NavigationViewItem)?.Tag?.ToString();

        if (tag is not null)
            NavigateTo(tag);
    }

    private void RefreshNavItems()
    {
        foreach (var item in NavView.MenuItems.OfType<NavigationViewItem>()
            .Concat(NavView.FooterMenuItems.OfType<NavigationViewItem>()))
        {
            item.Content = item.Tag?.ToString() switch
            {
                var t when t == AppConstants.NavTags.Home => RuntimeLocalizationHelper.App.GetString("NavHome.Content"),
                var t when t == AppConstants.NavTags.Changelog => RuntimeLocalizationHelper.App.GetString("NavChangelog.Content"),
                _ => item.Content
            };
        }
    }

    private void RefreshNavBuiltInItems()
    {
        var settingsText = RuntimeLocalizationHelper.App.GetString("String_NavSettings");
        var backText = RuntimeLocalizationHelper.App.GetString("String_NavBack");
        var paneOpenText = RuntimeLocalizationHelper.App.GetString("String_NavPaneOpen");
        var paneCloseText = RuntimeLocalizationHelper.App.GetString("String_NavPaneClose");

        if (NavView.SettingsItem is NavigationViewItem settingsNavItem)
        {
            settingsNavItem.Content = settingsText;
        }

        foreach (var item in UiTreeHelper.EnumerateVisualChildren<NavigationViewItem>(NavView))
        {
            if (ReferenceEquals(item, NavView.SettingsItem) ||
                item.Name == "SettingsNavViewItem" ||
                item.Name == "SettingsItem")
            {
                item.Content = settingsText;
            }
        }

        foreach (var button in UiTreeHelper.EnumerateVisualChildren<Button>(AppTitleBar))
        {
            if (button.Name == "PART_BackButton")
            {
                ToolTipService.SetToolTip(button, backText);
            }
            else if (button.Name == "PART_PaneToggleButton")
            {
                ToolTipService.SetToolTip(button, NavView.IsPaneOpen ? paneCloseText : paneOpenText);
            }
        }

        foreach (var button in UiTreeHelper.EnumerateVisualChildren<Button>(NavView))
        {
            if (button.Name == "BackButton")
            {
                ToolTipService.SetToolTip(button, backText);
            }
            else if (button.Name == "TogglePaneButton" ||
                     button.Name == "PaneToggleButton")
            {
                ToolTipService.SetToolTip(button, NavView.IsPaneOpen ? paneCloseText : paneOpenText);
            }
        }
    }

    public void NavigateTo(string tag, bool pushHistory = true, object? parameter = null)
    {
        Type? pageType = tag switch
        {
            "home" => typeof(HomePage),
            "changelog" => typeof(ChangelogPage),
            "settings" => typeof(SettingsPage),
            _ => null
        };
        if (pageType is null) return;
        if (ContentFrame.Content?.GetType() == pageType) return;

        if (pushHistory && _lastSidebarTag is not null && _lastSidebarTag != tag)
        {
            _history.Push(_lastSidebarTag);
        }

        _suppressNavigation = true;
        try
        {
            if (tag == "settings")
            {
                NavView.SelectedItem = NavView.SettingsItem;
            }
            else
            {
                var target = NavView.MenuItems.OfType<NavigationViewItem>()
                    .Concat(NavView.FooterMenuItems.OfType<NavigationViewItem>())
                    .FirstOrDefault(i => i.Tag?.ToString() == tag);
                if (target is not null && !ReferenceEquals(NavView.SelectedItem, target))
                    NavView.SelectedItem = target;
            }
        }
        finally
        {
            _suppressNavigation = false;
        }

        var transition = new SlideNavigationTransitionInfo
        {
            Effect = SlideNavigationTransitionEffect.FromBottom
        };

        try
        {
            var navigated = ContentFrame.Navigate(pageType, parameter, transition);
            if (!navigated)
            {
                ContentFrame.Content = (Page)Activator.CreateInstance(pageType)!;
            }
        }
        catch (Exception)
        {
            try
            {
                ContentFrame.Content = (Page)Activator.CreateInstance(pageType)!;
            }
            catch (Exception fbEx)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation failed: {fbEx}");
            }
        }

        _lastSidebarTag = tag;
        AppTitleBar.IsBackButtonEnabled = _history.Count > 0;
        EnqueueTitleBarFix();
    }

    public void UpdateBackButtonState()
    {
        AppTitleBar.IsBackButtonEnabled = _history.Count > 0;
        EnqueueTitleBarFix();
    }

    private int _minWidth = 800;
    private int _minHeight = 600;
    private IntPtr _hWnd;
    private SUBCLASSPROC? _subclassProc;
    private IntPtr _subclassId;

    private void SetWindowMinSize(int minWidth, int minHeight)
    {
        _minWidth = minWidth;
        _minHeight = minHeight;
        _hWnd = WindowNative.GetWindowHandle(this);
        _subclassProc = SubclassWndProc;
        SetWindowSubclass(_hWnd, _subclassProc, IntPtr.Zero, out _subclassId);
    }

    private IntPtr SubclassWndProc(
        IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam,
        IntPtr uIdSubclass, IntPtr dwRefData)
    {
        if (uMsg == AppConstants.WmGetMinMaxInfo)
        {
            var dpi = GetDpiForWindow(hWnd);
            var scale = dpi / AppConstants.DpiBase;
            var info = Marshal.PtrToStructure<MINMAXINFO>(lParam);
            info.ptMinTrackSize.x = (int)(_minWidth * scale);
            info.ptMinTrackSize.y = (int)(_minHeight * scale);
            Marshal.StructureToPtr(info, lParam, false);
        }
        return DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr SUBCLASSPROC(
        IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam,
        IntPtr uIdSubclass, IntPtr dwRefData);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern bool SetWindowSubclass(
        IntPtr hWnd, SUBCLASSPROC pfnSubclass,
        IntPtr uIdSubclass, out IntPtr pdwRefData);

    [DllImport("comctl32.dll")]
    private static extern IntPtr DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hWnd);
}