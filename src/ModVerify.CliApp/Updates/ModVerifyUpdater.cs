using System;
using System.Threading.Tasks;
using AET.ModVerify.App.Updates.Github;
using AnakinRaW.ApplicationBase;
using AnakinRaW.ApplicationBase.Update.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

    public async Task RunUpdateProcedure(ModVerifyUpdateMode mode)
    {
        _logger?.LogTrace("Running update procedure - '{mode}'", mode);

        // If we are in the check-only mode, GitHub check is sufficient.
        if (mode == ModVerifyUpdateMode.CheckOnly)
        {
            await CheckForUpdateAndReport().ConfigureAwait(false);
            return;
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