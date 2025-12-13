using System;
using System.Linq;
using AET.ModVerify.App.GameFinder;
using AET.ModVerify.App.Settings;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure;

namespace AET.ModVerify.App.ModSelectors;

internal class SettingsBasedModSelector(IServiceProvider serviceProvider)
{
    public VerificationTarget CreateInstallationDataFromSettings(GameInstallationsSettings settings)
    {
        var gameLocations = new ModSelectorFactory(serviceProvider)
            .CreateSelector(settings)
            .Select(settings, out var targetObject, out var engineType);

        if (gameLocations is null)
            throw new GameNotFoundException("Unable to get game locations");

        if (engineType is null)
            throw new InvalidOperationException("Engine type not specified.");

        return new VerificationTarget
        {
            Engine = engineType.Value,
            Location = gameLocations,
            Name = GetNameFromGameLocations(targetObject, gameLocations),
            Version = GetTargetVersion(targetObject)
        };
    }

    private static string GetNameFromGameLocations(IPlayableObject? targetObject, GameLocations gameLocations)
    {
        if (targetObject is not null)
            return targetObject.Name;

        var mod = gameLocations.ModPaths.FirstOrDefault();
        return mod ?? gameLocations.GamePath;
    }

    private static string? GetTargetVersion(IPlayableObject? targetObject)
    {
        // TODO: Implement version retrieval from targetObject if possible
        return null;
    }
}