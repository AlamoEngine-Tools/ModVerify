using AET.ModVerify.App.Reporting;
using AET.ModVerify.App.Settings;
using AET.ModVerify.Pipeline;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Settings;
using AnakinRaW.ApplicationBase;
using AnakinRaW.ApplicationBase.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using AET.ModVerify.App.GameFinder;
using AET.ModVerify.App.TargetSelectors;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace AET.ModVerify.App;

internal sealed class ModVerifyApplication(ModVerifyAppSettings settings, IServiceProvider services)
{
    private readonly ILogger? _logger = services.GetService<ILoggerFactory>()?.CreateLogger(typeof(ModVerifyApplication));
    private readonly IFileSystem _fileSystem = services.GetRequiredService<IFileSystem>();
    private readonly ModVerifyAppEnvironment _appEnvironment = services.GetRequiredService<ModVerifyAppEnvironment>();

    public async Task<int> RunAsync()
    {
        using (new UnhandledExceptionHandler(services))
        using (new UnobservedTaskExceptionHandler(services))
            return await RunCoreAsync().ConfigureAwait(false);
    }

    private async Task<int> RunCoreAsync()
    {
       _logger?.LogDebug("Raw command line: {CommandLine}", Environment.CommandLine);

        var interactive = settings.Interactive;
        try
        {
            return await RunModVerifyAsync().ConfigureAwait(false);
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


    private async Task<int> RunModVerifyAsync()
    {
        VerificationTarget verificationTarget;
        try
        {
            var targetSettings = settings.VerificationTargetSettings;
            verificationTarget = new VerificationTargetSelectorFactory(services)
                .CreateSelector(targetSettings)
                .Select(targetSettings);
        }
        catch (ArgumentException ex)
        {
            ConsoleUtilities.WriteApplicationFatalError(_appEnvironment.ApplicationName, 
                $"The specified arguments are not correct: {ex.Message}");
            _logger?.LogError(ex, "Invalid application arguments: {Message}", ex.Message);
            return ex.HResult;
        }
        catch (TargetNotFoundException ex)
        {
            ConsoleUtilities.WriteApplicationFatalError(_appEnvironment.ApplicationName, ex.Message);
            _logger?.LogError(ex, ex.Message);
            return ex.HResult;
        }
        catch (GameNotFoundException ex)
        {
            ConsoleUtilities.WriteApplicationFatalError(_appEnvironment.ApplicationName, 
                "Unable to find an installation of Empire at War or Forces of Corruption.");
            _logger?.LogError(ex, "Game not found: {Message}", ex.Message);
            return ex.HResult;
        }

        var reportSettings = CreateGlobalReportSettings(verificationTarget);

        _logger?.LogDebug("Verification taget: {Target}", verificationTarget);
        _logger?.LogTrace("Verify settings: {Settings}", settings);

        var allErrors = await VerifyTargetAsync(verificationTarget, reportSettings)
            .ConfigureAwait(false);

        // TODO: Refactor method to represent "verify" and "baseline" mode of the app.
        // Also display to user more prominently which mode is active.

        try
        {
            await ReportErrors(allErrors).ConfigureAwait(false);
        }
        catch (GameVerificationException e)
        {
            _logger?.LogInformation(ModVerifyConstants.ConsoleEventId,
                "The verification of {Target} completed with findings of the specified failure severity {Severity}", 
                verificationTarget.Name, settings.AppThrowsOnMinimumSeverity);
            return e.HResult;
        }

        if (!settings.CreateNewBaseline)
            return 0;

        await WriteBaselineAsync(verificationTarget, reportSettings, allErrors, settings.NewBaselinePath).ConfigureAwait(false);
        _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Baseline successfully created.");

        return 0;
    }

    private async Task<IReadOnlyCollection<VerificationError>> VerifyTargetAsync(
        VerificationTarget verificationTarget,
        GlobalVerifyReportSettings reportSettings)
    {
        var progressReporter = new VerifyConsoleProgressReporter(verificationTarget.Name);

        using var verifyPipeline = new GameVerifyPipeline(
            verificationTarget,
            settings.VerifyPipelineSettings,
            reportSettings,
            progressReporter,
            new EngineInitializeProgressReporter(verificationTarget.Engine),
            services);

        try
        {
            try
            {
                _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Verifying '{Target}'...", verificationTarget.Name);
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
            _logger?.LogError(e, "Verification failed: {Message}", e.Message);
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

    private async Task WriteBaselineAsync(
        VerificationTarget target,
        GlobalVerifyReportSettings reportSettings,
        IEnumerable<VerificationError> errors, 
        string baselineFile)
    {
        var baselineFactory = services.GetRequiredService<BaselineFactory>();
        var baseline = baselineFactory.CreateBaseline(target, reportSettings, errors);
        await baselineFactory.WriteBaselineAsync(baseline, baselineFile);
    }

    private GlobalVerifyReportSettings CreateGlobalReportSettings(VerificationTarget verificationTarget)
    {
        var baselineSelector = new BaselineSelector(settings, services);
        var baseline = baselineSelector.SelectBaseline(verificationTarget, out var baselinePath);

        if (baseline.Count > 0) 
            _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Using baseline '{Baseline}'", baselinePath);

        var suppressionsFile = settings.ReportSettings.SuppressionsPath;
        SuppressionList suppressions;

        if (string.IsNullOrEmpty(suppressionsFile))
            suppressions = SuppressionList.Empty;
        else
        {
            using var fs = _fileSystem.File.OpenRead(suppressionsFile);
            suppressions = SuppressionList.FromJson(fs);

            if (suppressions.Count > 0)
                _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Using suppressions from '{Suppressions}'", suppressionsFile);
        }


        return new GlobalVerifyReportSettings
        {
            Baseline = baseline,
            Suppressions = suppressions,
            MinimumReportSeverity = settings.ReportSettings.MinimumReportSeverity,
        };
    }
}