using AET.ModVerify.App.Resources.Baselines;
using AET.ModVerify.App.Settings;
using AET.ModVerify.Reporting.Baseline;
using AnakinRaW.ApplicationBase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AET.ModVerify.App.Reporting;

internal sealed class BaselineSelector(AppVerifySettings settings, IServiceProvider services)
{
    private readonly ILogger? _logger = services.GetService<ILoggerFactory>()?.CreateLogger(typeof(ModVerifyApplication));
    private readonly IBaselineFactory _baselineFactory = services.GetRequiredService<IBaselineFactory>();

    public VerificationBaseline SelectBaseline(VerificationTarget verificationTarget, out string? usedBaselinePath)
    {
        var baselinePath = settings.ReportSettings.BaselinePath;
        if (!string.IsNullOrEmpty(baselinePath))
        {
            try
            {
                usedBaselinePath = baselinePath;
                return _baselineFactory.ParseBaseline(baselinePath);
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

        if (settings.ReportSettings is { SearchBaselineLocally: false, UseDefaultBaseline: false })
        {
            _logger?.LogDebug(ModVerifyConstants.ConsoleEventId, 
                "No baseline path specified and local search is not enabled. Using empty baseline.");
            usedBaselinePath = null;
            return VerificationBaseline.Empty;
        }

        if (settings.IsInteractive) 
            return FindBaselineInteractive(verificationTarget, out usedBaselinePath);

        // If the application is not interactive, we only use a baseline file present in the directory of the verification target.
        return FindBaselineNonInteractive(verificationTarget, out usedBaselinePath);

    }

    private VerificationBaseline FindBaselineInteractive(VerificationTarget verificationTarget, out string? baselinePath)
    {
        // The application is in interactive mode. We apply the following lookup: 
        // 1. Use a baseline found in the directory of the verification target.
        // 2. Use a baseline found in the directory ModVerify executable.
        // 3. If the verification target is a mod, ask the user to apply the default game's baseline.
        // In any case ask the use if they want to use the located baseline file, or they wish to continue using none/empty.

        _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Searching for local baseline files...");

        if (!_baselineFactory.TryFindBaselineInDirectory(
                verificationTarget.Location.TargetPath,
                b => IsBaselineCompatible(b, verificationTarget),
                out var baseline,
                out baselinePath))
        {
            if (!_baselineFactory.TryFindBaselineInDirectory(
                    Environment.CurrentDirectory,
                    b => IsBaselineCompatible(b, verificationTarget), 
                    out baseline, 
                    out baselinePath))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("No baseline found locally.");
                Console.ResetColor();
                baselinePath = null;
                TryGetDefaultBaseline(verificationTarget.Engine, out baseline);
                return baseline ?? VerificationBaseline.Empty;
            }
        }

        Debug.Assert(baselinePath is not null && baseline is not null);

        return ShouldUseBaseline(baseline, baselinePath)
            ? baseline 
            : VerificationBaseline.Empty;
    }

    private static bool TryGetDefaultBaseline(
        GameEngineType engineType, 
        [NotNullWhen(true)] out VerificationBaseline? baseline)
    {
        baseline = null;
        if (engineType == GameEngineType.Eaw)
        {
            // TODO: EAW currently not implemented
            return false;
        }

        if (!ConsoleUtilities.UserYesNoQuestion($"Do you want to load the default baseline for game engine '{engineType}'?"))
            return false;

        try
        {
            baseline = LoadEmbeddedBaseline(engineType);
            return true;
        }
        catch (InvalidBaselineException)
        {
            throw new InvalidOperationException(
                "Invalid baseline packed along ModVerify App. Please reach out to the creators. Thanks!");
        }
    }

    internal static VerificationBaseline LoadEmbeddedBaseline(GameEngineType engineType)
    {
        var baselineFileName = $"baseline-{engineType.ToString().ToLower()}.json";
        var resourcePath = $"{typeof(BaselineResources).Namespace}.{baselineFileName}";

        using var baselineStream = typeof(BaselineSelector).Assembly.GetManifestResourceStream(resourcePath)!;
        return VerificationBaseline.FromJson(baselineStream);
    }

    private VerificationBaseline FindBaselineNonInteractive(VerificationTarget target, out string? usedPath)
    {
        if (_baselineFactory.TryFindBaselineInDirectory(
                target.Location.TargetPath,
                b => IsBaselineCompatible(b, target),
                out var baseline,
                out usedPath))
        {
            _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Automatically applying local baseline file '{Path}'.", usedPath);
            return baseline;
        }
        _logger?.LogTrace("No baseline file found in taget path '{TargetPath}'.", target.Location.TargetPath);
        usedPath = null;
        if (settings.ReportSettings.UseDefaultBaseline)
        {
            try
            {
                var defaultBaseline = LoadEmbeddedBaseline(target.Engine);
                _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Automatically applying default embedded baseline for engine '{Engine}'.", target.Engine);
                return defaultBaseline;
            }
            catch (InvalidBaselineException)
            {
                throw new InvalidOperationException(
                    "Invalid baseline packed along ModVerify App. Please reach out to the creators. Thanks!");
            }
        }
        return VerificationBaseline.Empty;
    }


    private static bool IsBaselineCompatible(VerificationBaseline baseline, VerificationTarget target)
    {
        return baseline.Target?.Engine == target.Engine;
    }

    private static bool ShouldUseBaseline(VerificationBaseline baseline, string baselinePath)
    {
        var sb = new StringBuilder("Found baseline ");
        if (baseline.Target is not null) 
            sb.Append($"for '{baseline.Target.Name}' ");

        sb.Append($"at '{baselinePath}'.");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(sb.ToString());

        return ConsoleUtilities.UserYesNoQuestion("Do you want to use it?");
    }
}