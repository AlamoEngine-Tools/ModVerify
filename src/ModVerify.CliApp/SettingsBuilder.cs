using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Settings;
using AET.ModVerify.Settings;
using AET.ModVerifyTool.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using AET.ModVerifyTool.Options.CommandLine;
using Microsoft.Extensions.Logging;

namespace AET.ModVerifyTool;

internal sealed class SettingsBuilder(IServiceProvider services)
{
    private readonly IFileSystem _fileSystem = services.GetRequiredService<IFileSystem>();
    private readonly ILogger? _logger =
        services.GetRequiredService<ILoggerFactory>()?.CreateLogger(typeof(SettingsBuilder));

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
        var output = Environment.CurrentDirectory;
        var outDir = verifyOptions.OutputDirectory;
        
        if (!string.IsNullOrEmpty(outDir))
            output = _fileSystem.Path.GetFullPath(_fileSystem.Path.Combine(Environment.CurrentDirectory, outDir!));

        return new ModVerifyAppSettings
        {
            VerifyPipelineSettings = new VerifyPipelineSettings
            {
                VerifierFactory = new GameVerifierFactory(),
                FailFast = verifyOptions.FailFast,
                GameVerifySettings = new GameVerifySettings
                {
                    IgnoreAsserts = verifyOptions.IgnoreAsserts,
                    ThrowsOnMinimumSeverity = GetVerifierMinimumThrowSeverity()
                }
            },
            AppThrowsOnMinimumSeverity = verifyOptions.MinimumFailureSeverity,
            GameInstallationsSettings = BuildInstallationSettings(verifyOptions),
            GlobalReportSettings = BuilderGlobalReportSettings(verifyOptions),
            ReportOutput = output
        };

        VerificationSeverity? GetVerifierMinimumThrowSeverity()
        {
            var minFailSeverity = verifyOptions.MinimumFailureSeverity;
            if (verifyOptions.FailFast)
            {
                if (minFailSeverity == null)
                {
                    _logger?.LogWarning($"Verification is configured to fail fast but 'minFailSeverity' is not specified. " +
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
                GameVerifySettings = new GameVerifySettings
                {
                    IgnoreAsserts = false,
                    ThrowsOnMinimumSeverity = null,
                },
                VerifierFactory = new GameVerifierFactory(),
                FailFast = false,
            },
            AppThrowsOnMinimumSeverity = null,
            GameInstallationsSettings = BuildInstallationSettings(baselineVerb),
            GlobalReportSettings = BuilderGlobalReportSettings(baselineVerb),
            NewBaselinePath = baselineVerb.OutputFile,
            ReportOutput = null, 
        };
    }

    private GlobalVerifyReportSettings BuilderGlobalReportSettings(BaseModVerifyOptions options)
    {
        return new GlobalVerifyReportSettings
        {
            Baseline = CreateBaseline(),
            Suppressions = CreateSuppressions(),
            MinimumReportSeverity = options.MinimumSeverity,
        };

        VerificationBaseline CreateBaseline()
        {
            // It does not make sense to create a baseline on another baseline.
            if (options is not VerifyVerbOption verifyOptions || string.IsNullOrEmpty(verifyOptions.Baseline))
                return VerificationBaseline.Empty;

            using var fs = _fileSystem.FileStream.New(verifyOptions.Baseline!, FileMode.Open, FileAccess.Read);

            try
            {
                return VerificationBaseline.FromJson(fs);
            }
            catch (IncompatibleBaselineException)
            {
                Console.WriteLine($"The baseline '{verifyOptions.Baseline}' is not compatible with with version of ModVerify." +
                                  $"{Environment.NewLine}Please generate a new baseline file or download the latest version." +
                                  $"{Environment.NewLine}");
                throw;
            }
        }

        SuppressionList CreateSuppressions()
        {
            if (options.Suppressions is null) 
                return SuppressionList.Empty;
            using var fs = _fileSystem.FileStream.New(options.Suppressions, FileMode.Open, FileAccess.Read);
            return SuppressionList.FromJson(fs);
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
            gamePath = _fileSystem.Path.GetFullPath(gamePath);


        string? fallbackGamePath = null;
        if (!string.IsNullOrEmpty(gamePath) && !string.IsNullOrEmpty(options.FallbackGamePath))
            fallbackGamePath = _fileSystem.Path.GetFullPath(options.FallbackGamePath);

        var autoPath = options.AutoPath;
        if (!string.IsNullOrEmpty(autoPath))
            autoPath = _fileSystem.Path.GetFullPath(autoPath);

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