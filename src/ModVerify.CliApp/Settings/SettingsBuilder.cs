using AET.ModVerify.App.Settings.CommandLine;
using AET.ModVerify.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace AET.ModVerify.App.Settings;

internal sealed class SettingsBuilder(IServiceProvider serviceProvider)
{
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
        ValidateVerb();
        var failFastSetting = GetFailFastSetting();
        return new AppVerifySettings(BuildReportSettings())
        {
            ReportDirectory = GetReportDirectory(),
            VerifierServiceSettings = new VerifierServiceSettings
            {
                ParallelVerifiers = verifyOptions.Parallel ? 4 : 1,
                VerifiersProvider = new DefaultGameVerifiersProvider(),
                FailFastSettings = failFastSetting,
                GameVerifySettings = new GameVerifySettings
                {
                    IgnoreAsserts = verifyOptions.IgnoreAsserts,
                    ThrowsOnMinimumSeverity = failFastSetting.IsFailFast 
                        ? failFastSetting.MinumumSeverity
                        // The app shall not make a specific verifier throw, but it should always run to completion.
                        : null 
                }
            },
            AppFailsOnMinimumSeverity = verifyOptions.MinimumFailureSeverity,
            VerificationTargetSettings = BuildTargetSettings(verifyOptions),
        };

        void ValidateVerb()
        {
            if (verifyOptions.SearchBaselineLocally && !string.IsNullOrEmpty(verifyOptions.Baseline))
            {
                var searchOption = typeof(VerifyVerbOption).GetOptionName(nameof(VerifyVerbOption.SearchBaselineLocally));
                var baselineOption = typeof(VerifyVerbOption).GetOptionName(nameof(VerifyVerbOption.Baseline));
                throw new AppArgumentException($"Options {searchOption} and {baselineOption} cannot be used together.");
            }

            if (verifyOptions.UseDefaultBaseline && !string.IsNullOrEmpty(verifyOptions.Baseline))
            {
                var useDefaultOption = typeof(VerifyVerbOption).GetOptionName(nameof(VerifyVerbOption.UseDefaultBaseline));
                var baselineOption = typeof(VerifyVerbOption).GetOptionName(nameof(VerifyVerbOption.Baseline));
                throw new AppArgumentException($"Options {useDefaultOption} and {baselineOption} cannot be used together.");
            }

            if (verifyOptions is { UseDefaultBaseline: true, SearchBaselineLocally: true })
            {
                var useDefaultOption = typeof(VerifyVerbOption).GetOptionName(nameof(VerifyVerbOption.UseDefaultBaseline));
                var searchOption = typeof(VerifyVerbOption).GetOptionName(nameof(VerifyVerbOption.SearchBaselineLocally));
                throw new AppArgumentException($"Options {useDefaultOption} and {searchOption} cannot be used together.");
            }

            if (verifyOptions is { FailFast: true, MinimumFailureSeverity: null })
            {
                var failFast = typeof(VerifyVerbOption).GetOptionName(nameof(VerifyVerbOption.FailFast));
                var minThrowSeverity = typeof(VerifyVerbOption).GetOptionName(nameof(VerifyVerbOption.MinimumFailureSeverity));
                throw new AppArgumentException($"Option {failFast} requires to set {minThrowSeverity}.");
            }
        }

        FailFastSetting GetFailFastSetting()
        {
            return !verifyOptions.FailFast
                ? FailFastSetting.NoFailFast 
                : new FailFastSetting(verifyOptions.MinimumFailureSeverity!.Value);
        }

        string GetReportDirectory()
        {
            return _fileSystem.Path.GetFullPath(_fileSystem.Path.Combine(
                Environment.CurrentDirectory, 
                verifyOptions.OutputDirectory ?? "ModVerifyResults"));
        }

        VerifyReportSettings BuildReportSettings()
        {
            return new VerifyReportSettings
            {
                BaselinePath = verifyOptions.Baseline,
                MinimumReportSeverity = verifyOptions.MinimumSeverity,
                SearchBaselineLocally = verifyOptions.SearchBaselineLocally,
                UseDefaultBaseline = verifyOptions.UseDefaultBaseline,
                SuppressionsPath = verifyOptions.Suppressions,
                Verbose = verifyOptions.Verbose
            };
        }
    }

    private AppBaselineSettings BuildFromCreateBaselineVerb(CreateBaselineVerbOption baselineVerb)
    {
        return new AppBaselineSettings(BuildReportSettings())
        {
            VerifierServiceSettings = new VerifierServiceSettings
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
                SuppressionsPath = baselineVerb.Suppressions,
                Verbose = baselineVerb.Verbose
            };
        }
    }

    private VerificationTargetSettings BuildTargetSettings(BaseModVerifyOptions options)
    {
        var separator = _fileSystem.Path.PathSeparator;

        var modPaths = new List<string>();
        if (!string.IsNullOrEmpty(options.ModPaths))
        {
            var split = options.ModPaths!.Split([separator], StringSplitOptions.RemoveEmptyEntries);
            modPaths.AddRange(split.Select(s => _fileSystem.Path.GetFullPath(s)));
        }

        var fallbackPaths = new List<string>();
        if (!string.IsNullOrEmpty(options.AdditionalFallbackPath))
        {
            var split = options.AdditionalFallbackPath!.Split([separator], StringSplitOptions.RemoveEmptyEntries);
            fallbackPaths.AddRange(split.Select(s => _fileSystem.Path.GetFullPath(s)));
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