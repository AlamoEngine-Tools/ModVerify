using AET.ModVerifyTool.ModSelectors;
using AET.ModVerifyTool.Options;
using AET.ModVerifyTool.Reporting;
using AET.ModVerifyTool.Updates;
using AnakinRaW.ApplicationBase;
using AnakinRaW.ApplicationBase.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AET.ModVerify;
using AET.ModVerify.Pipeline;
using AET.ModVerify.Reporting;
using PG.StarWarsGame.Engine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace AET.ModVerifyTool;

internal sealed class ModVerifyApplication(ModVerifyAppSettings settings, IServiceProvider services)
{
    private readonly ILogger? _logger = services.GetService<ILoggerFactory>()?.CreateLogger(typeof(ModVerifyApplication));
    private readonly IFileSystem _fileSystem = services.GetRequiredService<IFileSystem>();

    public async Task<int> Run()
    {
        using (new UnhandledExceptionHandler(services))
        using (new UnobservedTaskExceptionHandler(services))
            return await RunCore().ConfigureAwait(false);
    }

    private async Task<int> RunCore()
    {
       _logger?.LogDebug($"Raw command line: {Environment.CommandLine}");

        var interactive = false;
        try
        {
            interactive = settings.Interactive;

            if (!settings.Offline)
                await CheckForUpdate().ConfigureAwait(false);
            
            return await RunVerify().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger?.LogCritical(e, e.Message);
            ConsoleUtilities.WriteApplicationFatalError(ModVerifyConstants.AppNameString, e); 
            return e.HResult;
        }
        finally
        {
#if NET
            await Log.CloseAndFlushAsync();
#else
            Log.CloseAndFlush();
#endif
            if (interactive)
            {
                Console.WriteLine();
                ConsoleUtilities.WriteHorizontalLine('-');
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
            }
        }
    }


    private async Task<int> RunVerify()
    {
        var installData = new SettingsBasedModSelector(services)
            .CreateInstallationDataFromSettings(settings.GameInstallationsSettings);

        _logger?.LogDebug($"Verify install data: {installData}");
        _logger?.LogTrace($"Verify settings: {settings}");

        var allErrors = await Verify(installData).ConfigureAwait(false);

        try
        {
            await ReportErrors(allErrors).ConfigureAwait(false);
        }
        catch (GameVerificationException e)
        {
            return e.HResult;
        }

        if (!settings.CreateNewBaseline)
            return 0;

        await WriteBaseline(allErrors, settings.NewBaselinePath).ConfigureAwait(false);
        _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Baseline successfully created.");

        return 0;
    }

    private async Task<IReadOnlyCollection<VerificationError>> Verify(VerifyInstallationInformation installInformation)
    {
        var gameEngineService = services.GetRequiredService<IPetroglyphStarWarsGameEngineService>();
        var engineErrorReporter = new ConcurrentGameEngineErrorReporter();

        IStarWarsGameEngine gameEngine;

        try
        {
            var initProgress = new Progress<string>();
            var initProgressReporter = new EngineInitializeProgressReporter(initProgress);

            try
            {
                _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, $"Creating Game Engine '{installInformation.EngineType}'");
                gameEngine = await gameEngineService.InitializeAsync(
                    installInformation.EngineType,
                    installInformation.GameLocations,
                    engineErrorReporter,
                    initProgress,
                    false,
                    CancellationToken.None).ConfigureAwait(false);
                _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Game Engine created");
            }
            finally
            {
                initProgressReporter.Dispose();
            }
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Creating game engine failed: {e.Message}");
            throw;
        }

        var progressReporter = new VerifyConsoleProgressReporter(installInformation.Name);

        using var verifyPipeline = new GameVerifyPipeline(
            gameEngine,
            engineErrorReporter,
            settings.VerifyPipelineSettings,
            settings.GlobalReportSettings,
            progressReporter,
            services);

        try
        {
            try
            {
                _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, $"Verifying '{installInformation.Name}'...");
                await verifyPipeline.RunAsync().ConfigureAwait(false);
                progressReporter.Report(string.Empty, 1.0);
            }
            catch
            {
                progressReporter.ReportError("Verification failed", null);
                throw;
            }
            finally
            {
                progressReporter.Dispose();
            }
        }
        catch (OperationCanceledException)
        {
            _logger?.LogWarning(ModVerifyConstants.ConsoleEventId, "Verification stopped due to enabled failFast setting.");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Verification failed: {e.Message}");
            throw;
        }

        _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Finished verification");
        return verifyPipeline.FilteredErrors;
    }

    private async Task ReportErrors(IReadOnlyCollection<VerificationError> errors)
    {
        _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Reporting Errors...");

        var reportBroker = new VerificationReportBroker(services);

        await reportBroker.ReportAsync(errors);

        if (errors.Any(x => x.Severity >= settings.AppThrowsOnMinimumSeverity))
            throw new GameVerificationException(errors);
    }

    private async Task WriteBaseline(IEnumerable<VerificationError> errors, string baselineFile)
    {
        var baseline = new VerificationBaseline(settings.GlobalReportSettings.MinimumReportSeverity, errors);

        var fullPath = _fileSystem.Path.GetFullPath(baselineFile);
        _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, $"Writing Baseline to '{fullPath}'");

#if NET
        await 
#endif
        using var fs = _fileSystem.FileStream.New(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await baseline.ToJsonAsync(fs);
    }


    private async Task CheckForUpdate()
    {
        var updateChecker = new ModVerifyUpdater(services);

        _logger?.LogDebug("Checking for available update");

        try
        {
            var updateInfo = await updateChecker.CheckForUpdateAsync().ConfigureAwait(false);
            if (updateInfo.IsUpdateAvailable)
            {
                ConsoleUtilities.WriteHorizontalLine();

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("New Update Available!");
                Console.ResetColor();

                Console.WriteLine($"Version: {updateInfo.NewVersion}, Download here: {updateInfo.DownloadLink}");
                ConsoleUtilities.WriteHorizontalLine();
                Console.WriteLine();

            }
        }
        catch (Exception e)
        {
            _logger?.LogWarning(ModVerifyConstants.ConsoleEventId, $"Unable to check for updates due to an internal error: {e.Message}");
            _logger?.LogTrace(e, "Checking for update failed: " + e.Message);
        }
    }
}