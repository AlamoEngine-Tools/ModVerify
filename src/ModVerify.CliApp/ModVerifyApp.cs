using AET.ModVerify;
using AET.ModVerify.Reporting;
using AET.ModVerifyTool.ModSelectors;
using AET.ModVerifyTool.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using AET.ModVerify.Pipeline;
using AnakinRaW.CommonUtilities.SimplePipeline.Progress;

namespace AET.ModVerifyTool;

internal class ModVerifyApp(ModVerifyAppSettings settings, IServiceProvider services)
{
    private readonly ILogger? _logger = services.GetService<ILoggerFactory>()?.CreateLogger(typeof(ModVerifyApp));
    private readonly IFileSystem _fileSystem = services.GetRequiredService<IFileSystem>();

    public async Task<int> RunApplication()
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
        _logger?.LogInformation("Baseline successfully created.");

        return 0;
    }

    private async Task<IReadOnlyCollection<VerificationError>> Verify(VerifyGameInstallationData installData)
    {
        using var verifyPipeline = new GameVerifyPipeline(
            installData.EngineType,
            installData.GameLocations,
            settings.VerifyPipelineSettings,
            settings.GlobalReportSettings,
            new VerifyConsoleProgressReporter(),
            services);

        try
        {
            _logger?.LogInformation($"Verifying '{installData.Name}'...");
            await verifyPipeline.RunAsync().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger?.LogWarning("Verification stopped due to enabled failFast setting.");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Verification failed: {e.Message}");
            throw;
        }
        finally
        {
            _logger?.LogInformation("Finished verification");
        }

        return verifyPipeline.FilteredErrors;
    }

    private async Task ReportErrors(IReadOnlyCollection<VerificationError> errors)
    {
        _logger?.LogInformation("Reporting Errors...");
        
        var reportBroker = new VerificationReportBroker(services);

        await reportBroker.ReportAsync(errors);

        if (errors.Any(x => x.Severity >= settings.AppThrowsOnMinimumSeverity))
            throw new GameVerificationException(errors);
    }


    private async Task WriteBaseline(IEnumerable<VerificationError> errors, string baselineFile)
    {
        var baseline = new VerificationBaseline(settings.GlobalReportSettings.MinimumReportSeverity, errors);

        var fullPath = _fileSystem.Path.GetFullPath(baselineFile);
        _logger?.LogInformation($"Writing Baseline to '{fullPath}'");

#if NET
        await 
#endif
        using var fs = _fileSystem.FileStream.New(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await baseline.ToJsonAsync(fs);
    }
}

public class VerifyConsoleProgressReporter : IVerifyProgressReporter
{
    public void Report(string progressText, double progress, ProgressType type, VerifyProgressInfo detailedProgress)
    {
        if (type != VerifyProgress.ProgressType)
            return;


        Console.WriteLine(progressText);
    }
}