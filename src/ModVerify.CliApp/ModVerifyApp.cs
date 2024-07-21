using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using AET.ModVerify;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ModVerify.CliApp;

internal class ModVerifyApp(GameVerifySettings settings, IServiceProvider services)
{
    private readonly ILogger? _logger = services.GetService<ILoggerFactory>()?.CreateLogger(typeof(ModVerifyApp));
    private readonly IFileSystem _fileSystem = services.GetRequiredService<IFileSystem>();

    private IReadOnlyCollection<VerificationError> Errors { get; set; } = Array.Empty<VerificationError>();

    public async Task RunVerification(VerifyGameSetupData gameSetupData)
    {
        var verifyPipeline = BuildPipeline(gameSetupData);
        _logger?.LogInformation($"Verifying {gameSetupData.VerifyObject.Name}...");
        await verifyPipeline.RunAsync();
        _logger?.LogInformation("Finished Verifying");
    }

    private VerifyGamePipeline BuildPipeline(VerifyGameSetupData setupData)
    {
        return new ModVerifyPipeline(setupData.EngineType, setupData.GameLocations, settings, services);
    }

    public async Task WriteBaseline(string baselineFile)
    {
    }
}