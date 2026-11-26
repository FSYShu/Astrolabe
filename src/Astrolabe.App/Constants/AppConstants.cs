namespace Astrolabe.App.Constants;

internal static class AppConstants
{
    public const string AppName = "Astrolabe";
    public const string AppUserModelId = "Astrolabe";
    public const string AppUserModelIdPackaged = "Astrolabe_e6tahm9t1221g!Astrolabe";
    public const string AppIconPath = "Assets/app.ico";
    public const int DefaultWindowWidth = 1200;
    public const int DefaultWindowHeight = 800;
    public const int MinWindowWidth = 800;
    public const int MinWindowHeight = 600;
    public const double DpiBase = 96.0;
    public const int TitleBarButtonSize = 40;
    public const byte TitleBarHoverBgAlpha = 40;
    public const byte TitleBarPressedBgAlpha = 60;
    public const byte TitleBarInactiveFgAlpha = 150;
    public const int DwmwaUseImmersiveDarkMode20 = 20;
    public const int DwmwaUseImmersiveDarkMode19 = 19;
    public const uint WmGetMinMaxInfo = 0x0024;
    public const string AppUserModelFmtid = "9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3";
    public const int AppUserModelIdPid = 5;
    public const int AppUserModelIconPid = 12;

    public static class NavTags
    {
        public const string Home = "home";

        public const string Changelog = "changelog";
        public const string Settings = "settings";
    }

    public static class Themes
    {
        public const string System = "system";
        public const string Light = "light";
        public const string Dark = "dark";
    }

    public static class Languages
    {
        public const string System = "system";
        public const string ZhCn = "zh-cn";
        public const string EnUs = "en-us";
        public const string ZhPrefix = "zh";
    }

    public static class ConfigKeys
    {
        public const string AppTheme = "AppTheme";
        public const string AppThemeModule = "Astrolabe.UI";
        public const string AppLanguage = "app.language";
        public const string AppConfigModule = "Astrolabe";
    }
}