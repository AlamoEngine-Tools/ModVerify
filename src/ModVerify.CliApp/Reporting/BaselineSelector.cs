using AET.ModVerify.App.Resources.Baselines;
using AET.ModVerify.App.Settings;
using AET.ModVerify.Reporting;
using AnakinRaW.ApplicationBase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine;
using System;
using System.Diagnostics;

namespace AET.ModVerify.App.Reporting;

internal sealed class BaselineSelector(ModVerifyAppSettings settings, IServiceProvider services)
{
    private readonly ILogger? _logger = services.GetService<ILoggerFactory>()?.CreateLogger(typeof(ModVerifyApplication));
    private readonly BaselineFactory _baselineFactory = new(services);

    public VerificationBaseline SelectBaseline(VerificationTarget verificationTarget, out string? usedBaselinePath)
    {
        var baselinePath = settings.ReportSettings.BaselinePath;
        if (!string.IsNullOrEmpty(baselinePath))
        {
            try
            {
                usedBaselinePath = baselinePath;
                return _baselineFactory.CreateBaseline(baselinePath!);
            }
            catch (InvalidBaselineException e)
            {
                using (ConsoleUtilities.HorizontalLineSeparatedBlock('*'))
                {
                    Console.WriteLine($"The baseline '{baselinePath}' is not a valid baseline file: {e.Message}" +
                                      $"{Environment.NewLine}Please generate a new baseline file or download the latest version." +
                                      $"{Environment.NewLine}");
                }

                // For now, we bubble up this exception because we except users
                // to correctly specify their baselines through command line arguments.
                throw;
            }
        }

        if (!settings.ReportSettings.SearchBaselineLocally)
        {
            _logger?.LogDebug(ModVerifyConstants.ConsoleEventId, "No baseline path specified and local search is not enabled. Using empty baseline.");
            usedBaselinePath = null;
            return VerificationBaseline.Empty;
        }

        if (settings.Interactive) 
            return FindBaselineInteractive(verificationTarget, out usedBaselinePath);

        // If the application is not interactive, we only use a baseline file present in the directory of the verification target.
        return FindBaselineNonInteractive(verificationTarget.Location.TargetPath, out usedBaselinePath);

    }

    private VerificationBaseline FindBaselineInteractive(VerificationTarget verificationTarget, out string? baselinePath)
    {
        // The application is in interactive mode. We apply the following lookup: 
        // 1. Use a baseline found in the directory of the verification target.
        // 2. Use a baseline found in the directory ModVerify executable.
        // 3. If the verification target is a mod, ask the user to apply the default game's baseline.
        // In any case ask the use if they want to use the located baseline file, or they wish to continue using none/empty.

        _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Searching for local baseline files...");

        if (!_baselineFactory.TryCreateBaseline(verificationTarget.Location.TargetPath, out var baseline,
                out baselinePath))
        {
            if (!_baselineFactory.TryCreateBaseline("./", out baseline, out baselinePath))
            {
                // It does not make sense to load the game's default baselines if the user wants to verify the game,
                // as the verification result would always be empty (at least in a non-development scenario)
                if (verificationTarget.Location.ModPaths.Count == 0)
                {
                    _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "No local baseline file found.");
                    return VerificationBaseline.Empty;
                }

                Console.WriteLine("No baseline found locally.");
                return TryGetDefaultBaseline(verificationTarget.Engine, out baselinePath);
            }
        }

        Debug.Assert(baselinePath is not null);

        return ConsoleUtilities.UserYesNoQuestion($"ModVerify found the baseline file '{baselinePath}'. Do you want to use it?")
            ? baseline
            : VerificationBaseline.Empty;
    }

    private VerificationBaseline TryGetDefaultBaseline(GameEngineType engineType, out string? baselinePath)
    {
        baselinePath = null;
        if (engineType == GameEngineType.Eaw)
        {
            // TODO: EAW currently not implemented
            return VerificationBaseline.Empty;
        }

        if (!ConsoleUtilities.UserYesNoQuestion($"Do you want to load the default baseline for game engine '{engineType}'?"))
            return VerificationBaseline.Empty;

        baselinePath = $"{engineType} (Default)";

        try
        {
            return LoadEmbeddedBaseline(engineType);
        }
        catch (InvalidBaselineException)
        {
            throw new InvalidOperationException(
                "Invalid baseline packed along ModVerify App. Please reach out to the creators. Thanks!");
        }
    }

    internal VerificationBaseline LoadEmbeddedBaseline(GameEngineType engineType)
    {
        var baselineFileName = $"baseline-{engineType.ToString().ToLower()}.json";
        var resourcePath = $"{typeof(BaselineResources).Namespace}.{baselineFileName}";

        using var baselineStream = typeof(BaselineSelector).Assembly.GetManifestResourceStream(resourcePath)!;
        return VerificationBaseline.FromJson(baselineStream);
    }

    private VerificationBaseline FindBaselineNonInteractive(string targetPath, out string? usedPath)
    {
        if (_baselineFactory.TryCreateBaseline(targetPath, out var baseline, out usedPath))
        {
            _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Automatically applying local baseline file '{Path}'.", usedPath);
            return baseline;
        }
        _logger?.LogTrace("No baseline file found in taget path '{TargetPath}'.", targetPath);
        usedPath = null;
        return VerificationBaseline.Empty;
    }
}