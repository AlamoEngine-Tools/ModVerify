using AET.ModVerify.App.Resources.Baselines;
using AET.ModVerify.App.Settings;
using AET.ModVerify.Reporting.Baseline;
using AnakinRaW.ApplicationBase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AET.ModVerify.App.Reporting;

internal sealed class BaselineSelector(AppSettingsBase settings, IServiceProvider services)
{
    private readonly ILogger? _logger = services.GetService<ILoggerFactory>()?.CreateLogger(typeof(ModVerifyApplication));
    private readonly IBaselineFactory _baselineFactory = services.GetRequiredService<IBaselineFactory>();

    private bool IsCreatingBaseline => settings is AppBaselineSettings;

    public BaselineCollection SelectBaselines(VerificationTarget verificationTarget)
    {
        var report = settings.ReportSettings;
        var collected = new List<IdentifiedBaseline>();
        var seenIdentifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in report.BaselinePaths)
        {
            var entry = LoadExplicitBaseline(path);
            if (seenIdentifiers.Add(entry.Identifier))
                collected.Add(entry);
        }

        // In interactive mode, offer to discover a baseline near the target when none was supplied.
        if (settings.IsInteractive && collected.Count == 0 && TryFindBaselineInteractive(verificationTarget, out var found))
            collected.Add(found);

        // Loading the engine's default baseline is meaningless when creating a baseline for the game itself
        // (you'd be subtracting it from itself). Skip it in that case.
        var defaultBaselineApplicable = !(IsCreatingBaseline && verificationTarget.IsGame);

        if (report.UseDefaultBaseline)
        {
            if (!defaultBaselineApplicable)
            {
                _logger?.LogWarning(ModVerifyConstants.ConsoleEventId,
                    "Ignoring --useDefaultBaseline: it does not apply when creating a baseline for the game itself.");
            }
            else if (TryLoadEmbeddedBaseline(verificationTarget.Engine, out var defaultBaseline, out var defaultId))
            {
                collected.Add(new IdentifiedBaseline(defaultId, defaultBaseline, BaselineSource.EmbeddedDefault));
            }
        }
        else if (settings.IsInteractive && defaultBaselineApplicable)
        {
            // In interactive mode, offer the embedded default independently of any locally
            // discovered or explicitly supplied baselines — they're typically stacked.
            if (TryPromptForEmbeddedBaseline(verificationTarget.Engine, out var defaultBaseline, out var defaultId))
                collected.Add(new IdentifiedBaseline(defaultId, defaultBaseline, BaselineSource.EmbeddedDefault));
        }

        return new BaselineCollection(collected);
    }

    private IdentifiedBaseline LoadExplicitBaseline(string baselinePath)
    {
        try
        {
            return new IdentifiedBaseline(baselinePath, _baselineFactory.ParseBaseline(baselinePath), BaselineSource.File);
        }
        catch (InvalidBaselineException e)
        {
            using (ConsoleUtilities.HorizontalLineSeparatedBlock('*'))
            {
                Console.WriteLine($"The baseline '{baselinePath}' is not a valid baseline file: {e.Message}" +
                                  $"{Environment.NewLine}Please generate a new baseline file or download the latest version." +
                                  $"{Environment.NewLine}");
            }
            throw;
        }
    }

    private bool TryFindBaselineInteractive(VerificationTarget verificationTarget, [NotNullWhen(true)] out IdentifiedBaseline? found)
    {
        // 1. Use a baseline found in the directory of the verification target.
        // 2. Use a baseline found in the directory of the ModVerify executable.
        // Ask the user if they want to use the located baseline file.

        _logger?.LogInformation(ModVerifyConstants.ConsoleEventId, "Searching for local baseline files...");

        if (!_baselineFactory.TryFindBaselineInDirectory(
                verificationTarget.Location.TargetPath,
                b => IsBaselineCompatible(b, verificationTarget),
                out var baseline,
                out var baselinePath))
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
                found = null;
                return false;
            }
        }

        if (ShouldUseBaseline(baseline, baselinePath))
        {
            found = new IdentifiedBaseline(baselinePath, baseline, BaselineSource.File);
            return true;
        }
        found = null;
        return false;
    }

    private bool TryLoadEmbeddedBaseline(GameEngineType engineType,
        [NotNullWhen(true)] out VerificationBaseline? baseline,
        [NotNullWhen(true)] out string? identifier)
    {
        baseline = null;
        identifier = null;

        // TODO: EAW currently not implemented
        if (engineType == GameEngineType.Eaw)
            return false;

        try
        {
            baseline = LoadEmbeddedBaseline(engineType);
            identifier = MakeDefaultIdentifier(engineType);
            _logger?.LogInformation(ModVerifyConstants.ConsoleEventId,
                "Applying default embedded baseline for engine '{Engine}'.", engineType);
            return true;
        }
        catch (InvalidBaselineException)
        {
            throw new InvalidOperationException(
                "Invalid baseline packed along ModVerify App. Please reach out to the creators. Thanks!");
        }
    }

    private bool TryPromptForEmbeddedBaseline(GameEngineType engineType,
        [NotNullWhen(true)] out VerificationBaseline? baseline,
        [NotNullWhen(true)] out string? identifier)
    {
        baseline = null;
        identifier = null;

        // TODO: EAW currently not implemented
        if (engineType == GameEngineType.Eaw)
            return false;

        var question = IsCreatingBaseline
            ? $"Apply the default baseline for engine '{engineType}' as a base? Findings already covered by it will be excluded from your new baseline."
            : $"Do you want to load the default baseline for game engine '{engineType}'?";

        if (!ConsoleUtilities.UserYesNoQuestion(question, defaultAnswer: true))
            return false;

        return TryLoadEmbeddedBaseline(engineType, out baseline, out identifier);
    }

    internal static VerificationBaseline LoadEmbeddedBaseline(GameEngineType engineType)
    {
        var baselineFileName = $"baseline-{engineType.ToString().ToLower()}.json";
        var resourcePath = $"{typeof(BaselineResources).Namespace}.{baselineFileName}";

        using var baselineStream = typeof(BaselineSelector).Assembly.GetManifestResourceStream(resourcePath)!;
        return VerificationBaseline.FromJson(baselineStream);
    }

    internal static string MakeDefaultIdentifier(GameEngineType engineType)
        => $"<embedded-default:{engineType.ToString().ToLower()}>";

    private static bool IsBaselineCompatible(VerificationBaseline baseline, VerificationTarget target)
    {
        return baseline.Target?.Engine == target.Engine;
    }

    private bool ShouldUseBaseline(VerificationBaseline baseline, string baselinePath)
    {
        var sb = new StringBuilder("Found baseline ");
        if (baseline.Target is not null)
            sb.Append($"for '{baseline.Target.Name}' ");

        sb.Append($"at '{baselinePath}'.");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(sb.ToString());

        var question = IsCreatingBaseline
            ? "Use it as a base? Findings already covered by it will be excluded from your new baseline."
            : "Do you want to use it?";
        Console.ResetColor();
        return ConsoleUtilities.UserYesNoQuestion(question, defaultAnswer: true);
    }
}
