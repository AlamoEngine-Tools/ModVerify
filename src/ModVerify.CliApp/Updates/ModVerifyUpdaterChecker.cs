using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Semver;

namespace AET.ModVerifyTool.Updates;

internal sealed class ModVerifyUpdaterChecker
{
    private readonly ILogger? _logger;

    public ModVerifyUpdaterChecker(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public async Task<UpdateInfo> CheckForUpdateAsync()
    {
        var githubReleases = await DownloadReleaseList().ConfigureAwait(false);

        var branch = ModVerifyUpdaterInformation.BranchName;
        var latestRelease = githubReleases.FirstOrDefault(r => r.Branch == branch);

        if (latestRelease == null)
            throw new InvalidOperationException($"Unable to find a release for branch '{branch}'.");

        if (!SemVersion.TryParse(latestRelease.Tag, SemVersionStyles.Any, out var latestVersion))
            throw new InvalidOperationException($"Cannot create a version from tag '{latestRelease.Tag}'.");

        var currentVersion = ModVerifyUpdaterInformation.CurrentVersion;
        if (currentVersion is null)
            throw new InvalidOperationException("Unable to get current version.");

        if (SemVersion.ComparePrecedence(currentVersion, latestVersion) >= 0)
        {
            _logger?.LogDebug($"No update available - [Current Version = {currentVersion}], [Available Version = {latestVersion}]");
            return default;
        }

        _logger?.LogDebug($"Update available - [Current Version = {currentVersion}], [Available Version = {latestVersion}]");
        return new UpdateInfo
        {
            DownloadLink = ModVerifyUpdaterInformation.ModVerifyReleasesDownloadLink,
            IsUpdateAvailable = true,
            NewVersion = latestVersion.ToString()
        };
    }

    private static async Task<GithubReleaseList> DownloadReleaseList()
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ModVerifyUpdaterInformation.UserAgent);
        using var downloadStream = await httpClient.GetStreamAsync(ModVerifyUpdaterInformation.GithubReleasesApiLink).ConfigureAwait(false);

        using var jsonStream = new MemoryStream();
        await downloadStream.CopyToAsync(jsonStream).ConfigureAwait(false);
        jsonStream.Seek(0, SeekOrigin.Begin);

        var releases = await JsonSerializer.DeserializeAsync<GithubReleaseList>(jsonStream).ConfigureAwait(false);
        return releases ?? throw new InvalidOperationException("Unable to deserialize releases.");
    }
}