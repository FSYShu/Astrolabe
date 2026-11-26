using ShuRuk.Runtime.Contracts;
using ShuRuk.Runtime.Models;

namespace Astrolabe.App.Services;

public class TestModeService : ITestModeService
{
    public bool IsEnabled => false;
    public bool IsFillDataEnabled => false;
    public TestModeCoverage CoverageScope => TestModeCoverage.None;
    public string ChangelogRepoUrl => string.Empty;
    public string ChangelogSectionFilter => string.Empty;

    public event EventHandler<bool>? TestModeChanged;
    public event EventHandler<bool>? FillDataChanged;
    public event EventHandler<TestModeCoverage>? CoverageScopeChanged;

    public Task InitializeAsync() => Task.CompletedTask;

    public Task SetEnabledAsync(bool enabled)
    {
        TestModeChanged?.Invoke(this, enabled);
        return Task.CompletedTask;
    }

    public Task SetFillDataEnabledAsync(bool enabled)
    {
        FillDataChanged?.Invoke(this, enabled);
        return Task.CompletedTask;
    }

    public Task SetCoverageScopeAsync(TestModeCoverage scope)
    {
        CoverageScopeChanged?.Invoke(this, scope);
        return Task.CompletedTask;
    }

    public Task SetChangelogRepoUrlAsync(string repoUrl) => Task.CompletedTask;

    public Task SetChangelogSectionFilterAsync(string sectionFilter) => Task.CompletedTask;

    public bool IsPageCovered(TestModeCoverage coverage) => false;
}