using AET.ModVerify.App.Updates.Github;
using AET.ModVerify.App.Updates.SelfUpdate;
using AET.ModVerify.App.Utilities;
using AnakinRaW.ApplicationBase;
using AnakinRaW.ApplicationBase.Update.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;

namespace AET.ModVerify.App.Updates;

internal sealed class ModVerifyUpdater
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger? _logger;
    private readonly ModVerifyAppEnvironment _appEnvironment;

    public ModVerifyUpdater(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _appEnvironment = serviceProvider.GetRequiredService<ModVerifyAppEnvironment>();
    }

    public async Task RunUpdateProcedure(ApplicationUpdateOptions updateOptions, ModVerifyUpdateMode mode)
    {
        _logger?.LogTrace("Running update procedure - '{mode}'", mode);

        // If we are in the check-only mode, GitHub check is sufficient.
        if (mode == ModVerifyUpdateMode.CheckOnly)
        {
            await CheckForUpdateAndReport().ConfigureAwait(false);
            return;
        }
        
        await UpdateApplication(updateOptions, mode).ConfigureAwait(false);
    }

    private async Task UpdateApplication(ApplicationUpdateOptions updateOptions, ModVerifyUpdateMode mode)
    {
        if (!_appEnvironment.IsUpdatable(out var updatableEnvironment))
        {
            _logger?.LogWarning("Application is not updatable, yet we entered the update path. Checking only.");
            await CheckForUpdateAndReport().ConfigureAwait(false);
            return;
        }

        var updater = new ModVerifyApplicationUpdater(updatableEnvironment, _serviceProvider);

        var actualBranchName = updater.GetBranchNameFromRegistry(updateOptions.BranchName, false);
        var branch = updater.CreateBranch(actualBranchName, updateOptions.ManifestUrl);

        using (ConsoleUtilities.CreateHorizontalFrame(length: 40, startWithNewLine: true, newLineAtEnd: true))
        {
            var currentAction = "checking for update";
            try
            {
                var updateCheckSpinner = new ConsoleSpinnerOptions
                {
                    CompletedMessage = "Update check completed.",
                    RunningMessage = "Checking for update...",
                    FailedMessage = "Update check failed",
                    HideCursor = true
                };

                var updateCatalog =
                    await ConsoleSpinner.Run(async () =>
                            await updater.CheckForUpdateAsync(branch, CancellationToken.None),
                        updateCheckSpinner);


                if (updateCatalog.Action != UpdateCatalogAction.Update)
                {
                    Console.WriteLine("No update available.");
                    return;
                }

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"New update available: Version {updateCatalog.UpdateReference.Version}");
                Console.ResetColor();

                if (mode == ModVerifyUpdateMode.InteractiveUpdate)
                {
                    var shallUpdate = ConsoleUtilities.UserYesNoQuestion("Do you want to update now?");
                    if (!shallUpdate)
                        return;
                }

                currentAction = "updating";


                var updatingSpinner = new ConsoleSpinnerOptions
                {
                    RunningMessage = $"Updating {ModVerifyConstants.AppNameString}...",
                    HideCursor = true
                };
                await ConsoleSpinner.Run(async () =>
                        await updater.UpdateAsync(updateCatalog, CancellationToken.None),
                    updatingSpinner);
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Error while {currentAction}: {e.Message}");
                Console.ResetColor();
                _logger?.LogError(e, e.Message);
            }
        }
    }

    private async Task CheckForUpdateAndReport()
    {
        _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Checking for available update...");
        try
        {
            var updateInfo = await new GithubUpdateChecker(_serviceProvider)
                .CheckForUpdateAsync().ConfigureAwait(false);

            if (updateInfo.IsUpdateAvailable)
            {
                using (ConsoleUtilities.HorizontalLineSeparatedBlock(startWithNewLine: true, newLineAtEnd: true))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("New Update Available!");
                    Console.ResetColor();
                    Console.WriteLine($"Version: {updateInfo.NewVersion}, Download here: {updateInfo.DownloadLink}");
                }
            }
            else
            {
                _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "No update available.");
            }
        }
        catch (Exception e)
        {
            _logger?.LogWarning(ModVerifyConstants.ConsoleEventId, "Unable to check for updates due to an internal error: {message}", e.Message);
            _logger?.LogTrace(e, "Checking for update failed: {message}", e.Message);
        }
    }
}