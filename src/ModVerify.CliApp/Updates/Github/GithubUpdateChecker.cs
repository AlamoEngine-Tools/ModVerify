using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Semver;

namespace AET.ModVerify.App.Updates.Github;

internal class GithubUpdateChecker
{
    private readonly ILogger? _logger;
    private readonly ModVerifyAppEnvironment _appEnvironment;

    public GithubUpdateChecker(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _appEnvironment = serviceProvider.GetRequiredService<ModVerifyAppEnvironment>();
    }

    public async Task<GithubUpdateInfo> CheckForUpdateAsync()
    {
        var githubReleases = await DownloadReleaseList().ConfigureAwait(false);

        var branch = GithubUpdateConstants.BranchName;
        var latestRelease = githubReleases.FirstOrDefault(r => r.Branch == branch);

        if (latestRelease == null)
            throw new InvalidOperationException($"Unable to find a release for branch '{branch}'.");

        if (!SemVersion.TryParse(latestRelease.Tag, SemVersionStyles.Any, out var latestVersion))
            throw new InvalidOperationException($"Cannot create a version from tag '{latestRelease.Tag}'.");

        var currentVersion = _appEnvironment.AssemblyInfo.InformationalAsSemVer();
        if (currentVersion is null)
            throw new InvalidOperationException("Unable to get current version.");

        if (SemVersion.ComparePrecedence(currentVersion, latestVersion) >= 0)
        {
            _logger?.LogDebug("No update available - [Current Version = {CurrentVersion}], [Available Version = {LatestVersion}]", currentVersion, latestVersion);
            return default;
        }

        _logger?.LogDebug("Update available - [Current Version = {CurrentVersion}], [Available Version = {LatestVersion}]", currentVersion, latestVersion);
        return new GithubUpdateInfo
        {
            DownloadLink = GithubUpdateConstants.ModVerifyReleasesDownloadLink,
            IsUpdateAvailable = true,
            NewVersion = latestVersion.ToString()
        };
    }

    private static async Task<GithubReleaseList> DownloadReleaseList()
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(GithubUpdateConstants.UserAgent);
        using var downloadStream = await httpClient.GetStreamAsync(GithubUpdateConstants.GithubReleasesApiLink).ConfigureAwait(false);

        using var jsonStream = new MemoryStream();
        await downloadStream.CopyToAsync(jsonStream).ConfigureAwait(false);
        jsonStream.Seek(0, SeekOrigin.Begin);

        var releases = await JsonSerializer.DeserializeAsync<GithubReleaseList>(jsonStream).ConfigureAwait(false);
        return releases ?? throw new InvalidOperationException("Unable to deserialize releases.");
    }
}