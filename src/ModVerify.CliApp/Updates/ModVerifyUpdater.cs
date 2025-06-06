using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AnakinRaW.ApplicationBase.Update.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Semver;

namespace AET.ModVerify.App.Updates;

internal sealed class ModVerifyUpdater
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger? _logger;
    private readonly ModVerifyAppEnvironment _appEnvironment;

    public ModVerifyUpdater(ApplicationUpdateOptions updateOptions, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _appEnvironment = serviceProvider.GetRequiredService<ModVerifyAppEnvironment>();
    }



    public async Task<int> RunUpdateProcedure(ModVerifyUpdateMode mode)
    {
        //if (_settings.Offline)
        //{
        //    _logger?.LogTrace("App is running in offline mode. Nothing to do here.");
        //    return;
        //}

        //if (!IsUpdatable(out var updateEnv))
        //{
        //    await CheckUpdateGithub();
        //    return;
        //}



        _logger?.LogDebug("Checking for available update");

        //try
        //{
        //    var updateInfo = await updateChecker.CheckForUpdateAsync().ConfigureAwait(false);
        //    if (updateInfo.IsUpdateAvailable)
        //    {
        //        ConsoleUtilities.WriteHorizontalLine();

        //        Console.ForegroundColor = ConsoleColor.DarkGreen;
        //        Console.WriteLine("New Update Available!");
        //        Console.ResetColor();

        //        Console.WriteLine($"Version: {updateInfo.NewVersion}, Download here: {updateInfo.DownloadLink}");
        //        ConsoleUtilities.WriteHorizontalLine();
        //        Console.WriteLine();

        //    }
        //}
        //catch (Exception e)
        //{
        //    _logger?.LogWarning(ModVerifyConstants.ConsoleEventId, $"Unable to check for updates due to an internal error: {e.Message}");
        //    _logger?.LogTrace(e, "Checking for update failed: " + e.Message);
        //}

        return 0;
    }

    private async Task CheckUpdateGithub()
    {
        
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