using AET.ModVerify.App.Settings.CommandLine;
using AET.ModVerify.Pipeline;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace AET.ModVerify.App.Settings;

internal sealed class SettingsBuilder(IServiceProvider serviceProvider)
{
    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(SettingsBuilder));
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    public AppSettingsBase BuildSettings(BaseModVerifyOptions options)
    {
        switch (options)
        {
            case VerifyVerbOption verifyVerb:
                return BuildFromVerifyVerb(verifyVerb);
            case CreateBaselineVerbOption baselineVerb:
                return BuildFromCreateBaselineVerb(baselineVerb);
        }
        throw new NotSupportedException($"The option '{options.GetType().Name}' is not supported!");
    }

    private AppVerifySettings BuildFromVerifyVerb(VerifyVerbOption verifyOptions)
    {
        var failFastSetting = GetFailFastSetting();
        return new AppVerifySettings(BuildReportSettings(verifyOptions))
        {
            ReportDirectory = GetReportDirectory(),
            VerifyPipelineSettings = new VerifyPipelineSettings
            {
                ParallelVerifiers = verifyOptions.Parallel ? 4 : 1,
                VerifiersProvider = new DefaultGameVerifiersProvider(),
                FailFastSettings = failFastSetting,
                GameVerifySettings = new GameVerifySettings
                {
                    IgnoreAsserts = verifyOptions.IgnoreAsserts,
                    ThrowsOnMinimumSeverity = failFastSetting.IsFailFast 
                        ? failFastSetting.MinumumSeverity 
                        : verifyOptions.MinimumFailureSeverity
                }
            },
            AppFailsOnMinimumSeverity = verifyOptions.MinimumFailureSeverity,
            VerificationTargetSettings = BuildTargetSettings(verifyOptions),
        };


        FailFastSetting GetFailFastSetting()
        {
            if (!verifyOptions.FailFast)
                return FailFastSetting.NoFailFast;

            var minFailSeverity = verifyOptions.MinimumFailureSeverity;
            if (!minFailSeverity.HasValue)
            {
                _logger?.LogWarning(ModVerifyConstants.ConsoleEventId,
                    "Verification is configured to fail fast but 'minFailSeverity' is not specified. Using severity '{Severity}'.", VerificationSeverity.Information);
                minFailSeverity = VerificationSeverity.Information;
            }

            return new FailFastSetting(minFailSeverity.Value);
        }

        string GetReportDirectory()
        {
            return _fileSystem.Path.GetFullPath(_fileSystem.Path.Combine(
                Environment.CurrentDirectory, 
                verifyOptions.OutputDirectory ?? "ModVerifyResults"));
        }

        VerifyReportSettings BuildReportSettings(VerifyVerbOption options)
        {
            return new VerifyReportSettings
            {
                BaselinePath = options.Baseline,
                MinimumReportSeverity = options.MinimumSeverity,
                SearchBaselineLocally = options.SearchBaselineLocally,
                SuppressionsPath = options.Suppressions
            };
        }
    }

    private AppBaselineSettings BuildFromCreateBaselineVerb(CreateBaselineVerbOption baselineVerb)
    {
        return new AppBaselineSettings(BuildReportSettings())
        {
            VerifyPipelineSettings = new VerifyPipelineSettings
            {
                ParallelVerifiers = baselineVerb.Parallel ? 4 : 1,
                VerifiersProvider = new DefaultGameVerifiersProvider(),
                GameVerifySettings = GameVerifySettings.Default,
                FailFastSettings = FailFastSetting.NoFailFast,
            },
            VerificationTargetSettings = BuildTargetSettings(baselineVerb),
            NewBaselinePath = baselineVerb.OutputFile,
            WriteLocations = !baselineVerb.SkipLocation
        };

        AppReportSettings BuildReportSettings()
        {
            return new AppReportSettings
            {
                MinimumReportSeverity = baselineVerb.MinimumSeverity,
                SuppressionsPath = baselineVerb.Suppressions
            };
        }
    }

    private VerificationTargetSettings BuildTargetSettings(BaseModVerifyOptions options)
    {
        var modPaths = new List<string>();
        if (options.ModPaths is not null)
        {
            foreach (var mod in options.ModPaths)
            {
                if (!string.IsNullOrEmpty(mod))
                    modPaths.Add(_fileSystem.Path.GetFullPath(mod));
            }
        }

        var fallbackPaths = new List<string>();
        if (options.AdditionalFallbackPath is not null)
        {
            foreach (var fallback in options.AdditionalFallbackPath)
            {
                if (!string.IsNullOrEmpty(fallback))
                    fallbackPaths.Add(_fileSystem.Path.GetFullPath(fallback));
            }
        }

        var gamePath = options.GamePath;
        if (!string.IsNullOrEmpty(gamePath))
            gamePath = _fileSystem.Path.GetFullPath(gamePath!);


        string? fallbackGamePath = null;
        if (!string.IsNullOrEmpty(gamePath) && !string.IsNullOrEmpty(options.FallbackGamePath))
            fallbackGamePath = _fileSystem.Path.GetFullPath(options.FallbackGamePath!);

        var targetPath = options.TargetPath;
        if (!string.IsNullOrEmpty(targetPath))
            targetPath = _fileSystem.Path.GetFullPath(targetPath!);

        return new VerificationTargetSettings
        {
            TargetPath = targetPath,
            ModPaths = modPaths,
            GamePath = gamePath,
            FallbackGamePath = fallbackGamePath,
            AdditionalFallbackPaths = fallbackPaths,
            Engine = options.Engine
        };
    }
}