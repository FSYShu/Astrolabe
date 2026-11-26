using ShuRuk.Runtime.Contracts;
using ShuRuk.Runtime.Models;

namespace Astrolabe;

public sealed class AstrolabeModule : IModule
{
    public ModuleManifest Manifest => new()
    {
        Name = "astrolabe",
        DisplayName = "Astrolabe",
        Version = "0.1.0",
        Description = "File manager toolkit module / 文件管理工具集模块",
        Entry = "Astrolabe.AstrolabeModule",
        Permissions = new[] { "filesystem", "shell" },
        Source = ModuleSource.BuiltIn,
        Kind = ModuleKind.Module
    };

    public object? SettingsPage => null;

    public Task OnLoadAsync(IModuleContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task OnUnloadAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task OnPauseAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task OnResumeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}