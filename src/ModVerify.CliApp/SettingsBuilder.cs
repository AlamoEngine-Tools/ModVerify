using System;
using System.IO;
using System.IO.Abstractions;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using Microsoft.Extensions.DependencyInjection;

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
            GameVerifySettigns = BuildGameVerifySettings(options),
            PathToVerify = options.Path,
            Output = output,
            AdditionalFallbackPath = options.AdditionalFallbackPath,
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
}