using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using AET.ModVerify;
using AET.ModVerify.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;

namespace ModVerify.CliApp;

internal class ModVerifyApp(ModVerifyAppSettings settings, IServiceProvider services)
{
    private readonly ILogger? _logger = services.GetService<ILoggerFactory>()?.CreateLogger(typeof(ModVerifyApp));
    private readonly IFileSystem _fileSystem = services.GetRequiredService<IFileSystem>();

    public async Task<int> RunApplication()
    {
        var returnCode = 0;

        var gameSetupData = CreateGameSetupData(settings, services);
        var verifyPipeline = new ModVerifyPipeline(gameSetupData.EngineType, gameSetupData.GameLocations, settings.GameVerifySettigns, services);

        try
        {
            _logger?.LogInformation($"Verifying {gameSetupData.VerifyObject.Name}...");
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
        var fullPath = _fileSystem.Path.GetFullPath(baselineFile);
#if NET
        await 
#endif
        using var fs = _fileSystem.FileStream.New(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        var baseline = new VerificationBaseline(errors);
        await baseline.ToJsonAsync(fs);
    }

    private static VerifyGameSetupData CreateGameSetupData(ModVerifyAppSettings options, IServiceProvider services)
    {
        var selectionResult = new ModOrGameSelector(services).SelectModOrGame(options.PathToVerify);

        IList<string> mods = Array.Empty<string>();
        if (selectionResult.ModOrGame is IMod mod)
        {
            var traverser = services.GetRequiredService<IModDependencyTraverser>();
            mods = traverser.Traverse(mod)
                .Select(x => x.Mod)
                .OfType<IPhysicalMod>().Select(x => x.Directory.FullName)
                .ToList();
        }

        var fallbackPaths = new List<string>();
        if (selectionResult.FallbackGame is not null)
            fallbackPaths.Add(selectionResult.FallbackGame.Directory.FullName);

        if (!string.IsNullOrEmpty(options.AdditionalFallbackPath))
            fallbackPaths.Add(options.AdditionalFallbackPath);

        var gameLocations = new GameLocations(
            mods,
            selectionResult.ModOrGame.Game.Directory.FullName,
            fallbackPaths);

        return new VerifyGameSetupData
        {
            EngineType = GameEngineType.Foc,
            GameLocations = gameLocations,
            VerifyObject = selectionResult.ModOrGame,
        };
    }
}