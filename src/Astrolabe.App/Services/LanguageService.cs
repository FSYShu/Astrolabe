using Astrolabe.App.Constants;
using ShuRuk.Runtime.Contracts;
using ShuRuk.Runtime.Helpers;
using RuntimeLocalizationHelper = ShuRuk.Runtime.Helpers.LocalizationHelper;

namespace Astrolabe.App.Services;

public class LanguageService : ILanguageService
{
    public string CurrentLanguage { get; private set; } = AppConstants.Languages.System;

    public string ActiveLanguage { get; private set; } = AppConstants.Languages.System;

    public bool IsRestartPending { get; private set; }

    public List<(string Code, string DisplayName)> AvailableLanguages { get; } =
    [
        (AppConstants.Languages.System, "Follow system"),
        (AppConstants.Languages.ZhCn, "中文"),
        (AppConstants.Languages.EnUs, "English")
    ];

    public event Action? LanguageChanged;

    public LanguageService()
    {
    }

    public Task LoadAsync()
    {
        ActiveLanguage = CurrentLanguage;

        var lang = CurrentLanguage == AppConstants.Languages.System ? null : CurrentLanguage;

        if (lang is null)
        {
            var systemLang = System.Globalization.CultureInfo.CurrentUICulture.Name.ToLowerInvariant();
            lang = systemLang.StartsWith(AppConstants.Languages.ZhPrefix)
                ? AppConstants.Languages.ZhCn
                : AppConstants.Languages.EnUs;
        }

        RuntimeLocalizationHelper.App.SetLanguage(lang);
        RuntimeLocalizationHelper.Runtime.SetLanguage(lang);
        return Task.CompletedTask;
    }

    public Task SetLanguageAsync(string code)
    {
        if (CurrentLanguage == code) return Task.CompletedTask;
        CurrentLanguage = code;
        ActiveLanguage = code;

        var lang = code == AppConstants.Languages.System ? null : code;
        RuntimeLocalizationHelper.App.SetLanguage(lang);
        RuntimeLocalizationHelper.Runtime.SetLanguage(lang);

        var resolved = ResolveEffectiveLanguage(code);
        Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride =
            resolved == AppConstants.Languages.ZhCn ? "zh-CN" : "en-US";

        LanguageChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task SetLanguageForRestartAsync(string code)
    {
        CurrentLanguage = code;
        IsRestartPending = true;
        return Task.CompletedTask;
    }

    public Task RevertPendingLanguageChangeAsync()
    {
        if (!IsRestartPending) return Task.CompletedTask;
        CurrentLanguage = ActiveLanguage;
        IsRestartPending = false;
        return Task.CompletedTask;
    }

    public string ResolveEffectiveLanguage(string code)
    {
        if (code != AppConstants.Languages.System) return code;
        var systemLang = System.Globalization.CultureInfo.CurrentUICulture.Name.ToLowerInvariant();
        return systemLang.StartsWith(AppConstants.Languages.ZhPrefix)
            ? AppConstants.Languages.ZhCn
            : AppConstants.Languages.EnUs;
    }
}