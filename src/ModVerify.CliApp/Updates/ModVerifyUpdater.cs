using AET.ModVerify.App.Updates.Github;
using AnakinRaW.ApplicationBase;
using AnakinRaW.ApplicationBase.Update.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using AET.ModVerify.App.Updates.SelfUpdate;
using AET.ModVerify.App.Utilities;
using Vanara.PInvoke;

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

        var updater = new ModVerifyApplicationUpdater(mode, updatableEnvironment, _serviceProvider);

        var actualBranchName = updater.GetBranchNameFromRegistry(updateOptions.BranchName, false);
        var branch = updater.CreateBranch(actualBranchName, updateOptions.ManifestUrl);

        using (var block = ConsoleUtilities.CreateFixedHorizontalLineBlock('=', 40,
                   startWithNewLine: true,
                   newLineAtEnd: true))
        {
            block.WriteLine("This is inside the block.");
            block.WriteLine("The bottom line will move down as you write more lines.");
            // Simulate long-running output


            for (var i = 0; i < 3; i++)
            {
                await Task.Delay(500);
                await block.Writer.WriteAsync(i.ToString());
            }

            block.WriteLine();

            for (var i = 0; i < 3; i++)
            {
                await Task.Delay(500);
                block.WriteLine(i.ToString());
            }

            var spinnerOptions = new ConsoleSpinnerOptions
            {
                Writer = block.Writer,
                CompletedMessage = "DONE",
                RunningMessage = "Checking for update...",
                FailedMessage = "Update check failed",
                HideCursor = true
            };
            await ConsoleSpinner.Run(async () =>
            {
                await Task.Delay(2000); // Simulate some work
            }, spinnerOptions);

            block.WriteLine("456");
        }

        Console.WriteLine(123);
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