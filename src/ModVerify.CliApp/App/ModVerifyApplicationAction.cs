using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using AET.ModVerify.App.GameFinder;
using AET.ModVerify.App.Reporting;
using AET.ModVerify.App.Settings;
using AET.ModVerify.App.TargetSelectors;
using AET.ModVerify.Pipeline;
using AET.ModVerify.Reporting;
using AnakinRaW.ApplicationBase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AET.ModVerify.App;

internal abstract class ModVerifyApplicationAction<T> : IModVerifyAppAction where T : AppSettingsBase
{
    private readonly ModVerifyAppEnvironment _appEnvironment;
    private readonly IFileSystem _fileSystem;

    protected T Settings { get; }

    protected IServiceProvider ServiceProvider { get; }
    
    protected ILogger? Logger { get; }

    protected ModVerifyApplicationAction(T settings, IServiceProvider serviceProvider)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _appEnvironment = ServiceProvider.GetRequiredService<ModVerifyAppEnvironment>();
        _fileSystem = ServiceProvider.GetRequiredService<IFileSystem>();
    }

    protected virtual void PrintAction(VerificationTarget target)
    {
    }
    
    public async Task<int> ExecuteAsync()
    {
        VerificationTarget verificationTarget;
        try
        {
            var targetSettings = Settings.VerificationTargetSettings;
            verificationTarget = new VerificationTargetSelectorFactory(ServiceProvider)
                .CreateSelector(targetSettings)
                .Select(targetSettings);
        }
        catch (ArgumentException ex)
        {
            ConsoleUtilities.WriteApplicationFatalError(_appEnvironment.ApplicationName,
                $"The specified arguments are not correct: {ex.Message}");
            Logger?.LogError(ex, "Invalid application arguments: {Message}", ex.Message);
            return ex.HResult;
        }
        catch (TargetNotFoundException ex)
        {
            ConsoleUtilities.WriteApplicationFatalError(_appEnvironment.ApplicationName, ex.Message);
            Logger?.LogError(ex, ex.Message);
            return ex.HResult;
        }
        catch (GameNotFoundException ex)
        {
            ConsoleUtilities.WriteApplicationFatalError(_appEnvironment.ApplicationName,
                "Unable to find an installation of Empire at War or Forces of Corruption.");
            Logger?.LogError(ex, "Game not found: {Message}", ex.Message);
            return ex.HResult;
        }

        PrintAction(verificationTarget);

        var allErrors = await VerifyTargetAsync(verificationTarget)
            .ConfigureAwait(false);

        return await ProcessVerifyFindings(verificationTarget, allErrors);
    }

    protected abstract Task<int> ProcessVerifyFindings(
        VerificationTarget verificationTarget, 
        IReadOnlyCollection<VerificationError> allErrors);
    
    protected abstract VerificationBaseline GetBaseline(VerificationTarget verificationTarget);

    private async Task<IReadOnlyCollection<VerificationError>> VerifyTargetAsync(VerificationTarget verificationTarget)
    {
        var progressReporter = new VerifyConsoleProgressReporter(verificationTarget.Name, Settings.ReportSettings);

        var baseline = GetBaseline(verificationTarget);
        var suppressions = GetSuppressions();

        using var verifyPipeline = new GameVerifyPipeline(
            verificationTarget,
            Settings.VerifyPipelineSettings,
            progressReporter,
            new EngineInitializeProgressReporter(verificationTarget.Engine),
            baseline,
            suppressions,
            ServiceProvider);

        try
        {
            Logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Verifying '{Target}'...", verificationTarget.Name);
            await verifyPipeline.RunAsync().ConfigureAwait(false);
            progressReporter.Report(string.Empty, 1.0);
        }
        catch (OperationCanceledException)
        {
            Logger?.LogWarning(ModVerifyConstants.ConsoleEventId, "Verification stopped due to enabled failFast setting.");
        }
        catch (Exception e)
        {
            progressReporter.ReportError("Verification failed!", e.Message);
            Logger?.LogError(e, "Verification failed: {Message}", e.Message);
            throw;
        }
        finally
        {
            progressReporter.Dispose();
        }

        Logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Finished verification");
        return verifyPipeline.FilteredErrors;
    }

    private SuppressionList GetSuppressions()
    {
        var suppressionsFile = Settings.ReportSettings.SuppressionsPath;
        SuppressionList suppressions;
        if (string.IsNullOrEmpty(suppressionsFile))
            suppressions = SuppressionList.Empty;
        else
        {
            using var fileStream = _fileSystem.File.OpenRead(suppressionsFile);
            suppressions = SuppressionList.FromJson(fileStream);
            if (suppressions.Count > 0)
                Logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Using suppressions from '{Suppressions}'", suppressionsFile);
        }
        return suppressions;
    }
}