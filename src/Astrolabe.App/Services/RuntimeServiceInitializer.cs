using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using ShuRuk.Runtime.Contracts;
using ShuRuk.Runtime.Core;

namespace Astrolabe.App.Services;

public static class RuntimeServiceInitializer
{
    public static void Initialize(Window window)
    {
        var services = new ServiceCollection();

        var languageService = new LanguageService();
        languageService.LoadAsync().Wait();
        var themeService = new ThemeService();
        var testModeService = new TestModeService();
        var appContext = new AppContext(window);

        services.AddSingleton<ILanguageService>(languageService);
        services.AddSingleton<IThemeService>(themeService);
        services.AddSingleton<ITestModeService>(testModeService);

        App.Services = services.BuildServiceProvider();

        RuntimeServices.LanguageService = languageService;
        RuntimeServices.ThemeService = themeService;
        RuntimeServices.TestModeService = testModeService;
        RuntimeServices.AppContext = appContext;
    }
}