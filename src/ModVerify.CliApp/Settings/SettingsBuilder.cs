using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AET.ModVerify.App.Settings.CommandLine;
using AET.ModVerify.App.Utilities;
using AET.ModVerify.Pipeline;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AET.ModVerify.App.Settings;

internal sealed class SettingsBuilder(IServiceProvider serviceProvider)
{
    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(SettingsBuilder));
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    public ModVerifyAppSettings BuildSettings(BaseModVerifyOptions options)
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

    private ModVerifyAppSettings BuildFromVerifyVerb(VerifyVerbOption verifyOptions)
    {
        return new ModVerifyAppSettings
        {
            VerifyPipelineSettings = new VerifyPipelineSettings
            {
                ParallelVerifiers = verifyOptions.Parallel ? 4 : 1,
                VerifiersProvider = new DefaultGameVerifiersProvider(),
                FailFast = verifyOptions.FailFast,
                GameVerifySettings = new GameVerifySettings
                {
                    IgnoreAsserts = verifyOptions.IgnoreAsserts,
                    ThrowsOnMinimumSeverity = GetVerifierMinimumThrowSeverity()
                }
            },
            AppThrowsOnMinimumSeverity = verifyOptions.MinimumFailureSeverity,
            GameInstallationsSettings = BuildInstallationSettings(verifyOptions),
            ReportSettings = BuildReportSettings(verifyOptions),
        };

        VerificationSeverity? GetVerifierMinimumThrowSeverity()
        {
            var minFailSeverity = verifyOptions.MinimumFailureSeverity;
            if (verifyOptions.FailFast)
            {
                if (minFailSeverity == null)
                {
                    _logger?.LogWarning(ModVerifyConstants.ConsoleEventId, 
                        $"Verification is configured to fail fast but 'minFailSeverity' is not specified. " +
                        $"Using severity '{VerificationSeverity.Information}'.");
                    minFailSeverity = VerificationSeverity.Information;
                }

                return minFailSeverity;
            }

            // Only in a failFast scenario we want the verifier to throw.
            // In a normal run, the verifier should simply store the error.
            return null;
        }
    }

    private ModVerifyAppSettings BuildFromCreateBaselineVerb(CreateBaselineVerbOption baselineVerb)
    {
        return new ModVerifyAppSettings
        {
            VerifyPipelineSettings = new VerifyPipelineSettings
            {
                ParallelVerifiers = baselineVerb.Parallel ? 4 : 1,
                GameVerifySettings = new GameVerifySettings
                {
                    IgnoreAsserts = false,
                    ThrowsOnMinimumSeverity = null,
                },
                VerifiersProvider = new DefaultGameVerifiersProvider(),
                FailFast = false,
            },
            AppThrowsOnMinimumSeverity = null,
            GameInstallationsSettings = BuildInstallationSettings(baselineVerb),
            ReportSettings = BuildReportSettings(baselineVerb),
            NewBaselinePath = baselineVerb.OutputFile,
        };
    }

    private static ModVerifyReportSettings BuildReportSettings(BaseModVerifyOptions options)
    {
        var baselinePath = (options as VerifyVerbOption)?.Baseline;

        return new ModVerifyReportSettings
        {
            BaselinePath = baselinePath,
            MinimumReportSeverity = options.MinimumSeverity,
            SearchBaselineLocally = SearchLocally(options),
            SuppressionsPath = options.Suppressions
        };

        static bool SearchLocally(BaseModVerifyOptions o)
        {
            if (o is not VerifyVerbOption v)
                return false;
            return v.SearchBaselineLocally || v.LaunchedWithoutArguments();
        }
    }

    private GameInstallationsSettings BuildInstallationSettings(BaseModVerifyOptions options)
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

        var autoPath = options.AutoPath;
        if (!string.IsNullOrEmpty(autoPath))
            autoPath = _fileSystem.Path.GetFullPath(autoPath!);

        return new GameInstallationsSettings
        {
            AutoPath = autoPath,
            ModPaths = modPaths,
            GamePath = gamePath,
            FallbackGamePath = fallbackGamePath,
            AdditionalFallbackPaths = fallbackPaths,
            EngineType = options.GameType
        };
    }
}