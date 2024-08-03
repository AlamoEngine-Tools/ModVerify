using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using Microsoft.Extensions.DependencyInjection;
using ModVerify.CliApp.Options;

namespace ModVerify.CliApp;

internal class SettingsBuilder(IServiceProvider services)
{
    private readonly IFileSystem _fileSystem = services.GetRequiredService<IFileSystem>();

    public ModVerifyAppSettings BuildSettings(ModVerifyOptions options)
    {
        var output = Environment.CurrentDirectory;
        if (!string.IsNullOrEmpty(options.Output))
            output = _fileSystem.Path.GetFullPath(_fileSystem.Path.Combine(Environment.CurrentDirectory, options.Output));

        
        return new ModVerifyAppSettings
        {
            GameVerifySettings = BuildGameVerifySettings(options),
            GameInstallationsSettings = BuildInstallationSettings(options),
            Output = output,
            NewBaselinePath = options.NewBaselineFile
        };
    }

    private GameVerifySettings BuildGameVerifySettings(ModVerifyOptions options)
    {
        var settings = GameVerifySettings.Default;
        
        return settings with
        {
            GlobalReportSettings = BuilderGlobalReportSettings(options),
            AbortSettings = BuildAbortSettings(options)
        };
    }

    private VerificationAbortSettings BuildAbortSettings(ModVerifyOptions options)
    {
        return new VerificationAbortSettings
        {
            FailFast = options.FailFast,
            MinimumAbortSeverity = options.MinimumFailureSeverity ?? VerificationSeverity.Information,
            ThrowsGameVerificationException = options.MinimumFailureSeverity.HasValue || options.FailFast
        };
    }

    private GlobalVerificationReportSettings BuilderGlobalReportSettings(ModVerifyOptions options)
    {
        var baseline = VerificationBaseline.Empty;
        var suppressions = SuppressionList.Empty;
        
        if (options.Baseline is not null)
        {
            using var fs = _fileSystem.FileStream.New(options.Baseline, FileMode.Open, FileAccess.Read);
            baseline = VerificationBaseline.FromJson(fs);
        }

        if (options.Suppressions is not null)
        {
            using var fs = _fileSystem.FileStream.New(options.Suppressions, FileMode.Open, FileAccess.Read);
            suppressions = SuppressionList.FromJson(fs);
        }
        
        return new GlobalVerificationReportSettings
        {
            Baseline = baseline,
            Suppressions = suppressions,
            MinimumReportSeverity = VerificationSeverity.Information,
        };
    }

    private GameInstallationsSettings BuildInstallationSettings(ModVerifyOptions options)
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