using System;
using ModVerify.CliApp.Options;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure;

namespace ModVerify.CliApp.ModSelectors;

internal class ManualModSelector(IServiceProvider serviceProvider) : ModSelectorBase(serviceProvider)
{
    public override GameLocations Select(GameInstallationsSettings settings, out IPhysicalPlayableObject? targetObject,
        out GameEngineType? actualEngineType)
    {
        actualEngineType = settings.EngineType;
        targetObject = null;

        if (!actualEngineType.HasValue)
            throw new ArgumentException("Unable to determine game type. Use --type argument to set the game type.");

        if (string.IsNullOrEmpty(settings.GamePath))
            throw new ArgumentException("Argument --game must be set.");

        return new GameLocations(
            settings.ModPaths,
            settings.GamePath,
            GetFallbackPaths(settings.FallbackGamePath, settings.AdditionalFallbackPaths));
    }
}