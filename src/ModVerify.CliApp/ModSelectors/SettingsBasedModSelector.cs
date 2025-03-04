using System;
using System.Linq;
using AET.ModVerifyTool.Options;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure;

namespace AET.ModVerifyTool.ModSelectors;

internal class SettingsBasedModSelector(IServiceProvider serviceProvider)
{
    public VerifyGameInstallationData CreateInstallationDataFromSettings(GameInstallationsSettings settings)
    {
        var gameLocations = new ModSelectorFactory(serviceProvider)
            .CreateSelector(settings)
            .Select(settings, out var targetObject, out var engineType);

        if (gameLocations is null)
            throw new GameNotFoundException("Unable to get game locations");

        if (engineType is null)
            throw new InvalidOperationException("Engine type not specified.");

        return new VerifyGameInstallationData
        {
            EngineType = engineType.Value,
            GameLocations = gameLocations,
            Name = GetNameFromGameLocations(targetObject, gameLocations, engineType.Value)
        };
    }

    private static string GetNameFromGameLocations(IPlayableObject? targetObject, GameLocations gameLocations, GameEngineType engineType)
    {
        if (targetObject is not null)
            return targetObject.Name;

        var mod = gameLocations.ModPaths.FirstOrDefault();
        return mod ?? gameLocations.GamePath;
    }
}