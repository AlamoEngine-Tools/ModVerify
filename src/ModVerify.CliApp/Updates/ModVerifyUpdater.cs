using AET.ModVerify.App.Updates.Github;
using AET.ModVerify.App.Updates.SelfUpdate;
using AET.ModVerify.App.Utilities;
using AnakinRaW.ApplicationBase;
using AnakinRaW.ApplicationBase.Update.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

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

        {
            var b = ConsoleUtilities.UserYesNoQuestion("a");
            Console.WriteLine(b.ToString());
        }
        
        using (ConsoleUtilities.CreateHorizontalFrame(length: 40,
                   startWithNewLine: true,
                   newLineAtEnd: true))
        {
            Console.WriteLine("This is inside the block.");
            Console.WriteLine("The bottom line will move down as you write more lines.");

            
            var b = ConsoleUtilities.UserYesNoQuestion("a");
            Console.WriteLine(b.ToString());
            for (var i = 0; i < 3; i++)
            {
                await Task.Delay(500);
                Console.Write(i.ToString());
            }


            Console.Write("YourInput:");
            var a = Console.ReadLine();

            Console.WriteLine(a);


            for (var i = 0; i < 3; i++)
            {
                await Task.Delay(500);
                await Console.Out.WriteLineAsync(i.ToString());
            }

            var spinnerOptions = new ConsoleSpinnerOptions
            {
                CompletedMessage = "DONE",
                RunningMessage = "Checking for update...",
                FailedMessage = "Update check failed",
                HideCursor = true
            };

            try
            {
                await ConsoleSpinner.Run(async () =>
                {
                    await Task.Delay(2000); // Simulate some work
                    throw new Exception("Test");
                }, spinnerOptions);
            }
            catch (Exception e)
            {
                _logger?.LogError(e, e.Message);
            }


            //var currentAction = "checking for update";
            //try
            //{

            //    var updateCheckSpinner = new ConsoleSpinnerOptions
            //    {
            //        Writer = block.Writer,
            //        CompletedMessage = "Update check completed.",
            //        RunningMessage = "Checking for update...",
            //        FailedMessage = "Update check failed",
            //        HideCursor = true
            //    };

            //    var updateCatalog =
            //        await ConsoleSpinner.Run(async () => await updater.CheckForUpdateAsync(branch, CancellationToken.None),
            //            updateCheckSpinner);


            //    if (updateCatalog is null || updateCatalog.Action != UpdateCatalogAction.Update)
            //        return;


            //    if (mode == ModVerifyUpdateMode.InteractiveUpdate)
            //    {

            //    }

            //    currentAction = "updating";
            //    await updater.UpdateAsync(updateCatalog);
            //}
            //catch (Exception e)
            //{
            //    block.WriteLine("TEST");
            //    Console.ForegroundColor = ConsoleColor.DarkRed;
            //    block.WriteLine($"Error while {currentAction}: {e.Message}");
            //    _logger?.LogError(e, "Unable to check for updates: {error}", e.Message);
            //    Console.ResetColor();
            //    block.WriteLine();
            //}
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