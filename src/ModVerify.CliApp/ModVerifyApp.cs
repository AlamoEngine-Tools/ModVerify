using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using AET.ModVerify;
using AET.ModVerify.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModVerify.CliApp.ModSelectors;
using ModVerify.CliApp.Options;

namespace ModVerify.CliApp;

internal class ModVerifyApp(ModVerifyAppSettings settings, IServiceProvider services)
{
    private readonly ILogger? _logger = services.GetService<ILoggerFactory>()?.CreateLogger(typeof(ModVerifyApp));
    private readonly IFileSystem _fileSystem = services.GetRequiredService<IFileSystem>();

    public async Task<int> RunApplication()
    {
        var returnCode = 0;

        var installData = new SettingsBasedModSelector(services).CreateInstallationDataFromSettings(settings.GameInstallationsSettings);

        _logger?.LogInformation($"Verify install data: {installData}");

        var verifyPipeline = new ModVerifyPipeline(installData.EngineType, installData.GameLocations, settings.GameVerifySettings, services);

        try
        {
            _logger?.LogInformation($"Verifying '{installData.Name}'...");
            await verifyPipeline.RunAsync().ConfigureAwait(false);
            _logger?.LogInformation("Finished Verifying");
        }
        catch (GameVerificationException e)
        {
            returnCode = e.HResult;
        }

        if (settings.CreateNewBaseline)
            await WriteBaseline(verifyPipeline.Errors, settings.NewBaselinePath).ConfigureAwait(false);

        return returnCode;
    }

    private async Task WriteBaseline(IEnumerable<VerificationError> errors, string baselineFile)
    {
        var currentBaseline = settings.GameVerifySettings.GlobalReportSettings.Baseline;

        var newBaseline = currentBaseline.MergeWith(errors);

        var fullPath = _fileSystem.Path.GetFullPath(baselineFile);
#if NET
        await 
#endif
        using var fs = _fileSystem.FileStream.New(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await newBaseline.ToJsonAsync(fs);
    }
}