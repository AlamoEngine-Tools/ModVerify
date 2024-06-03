using System;
using System.Collections.Generic;
using System.Linq;
using PG.StarWarsGame.Files.MEG.Files;

namespace PG.StarWarsGame.Engine.Repositories;

// EaW file lookup works slightly different!
internal class FocGameRepository : GameRepository
{
    public override GameEngineType EngineType => GameEngineType.Foc;

    public FocGameRepository(GameLocations gameLocations, IServiceProvider serviceProvider) : base(gameLocations, serviceProvider)
    {
        if (gameLocations == null)
            throw new ArgumentNullException(nameof(gameLocations));

        var megsToConsider = new List<IMegFile>();

        // We assume that the first fallback path (if present at all) is always Empire at War.
        var firstFallback = FallbackPaths.FirstOrDefault();
        if (firstFallback is not null)
        {
            var eawMegs = LoadMegArchivesFromXml(firstFallback);
            var eawPatch = LoadMegArchive(FileSystem.Path.Combine(firstFallback, "Data\\Patch.meg"));
            var eawPatch2 = LoadMegArchive(FileSystem.Path.Combine(firstFallback, "Data\\Patch2.meg"));
            var eaw64Patch = LoadMegArchive(FileSystem.Path.Combine(firstFallback, "Data\\64Patch.meg"));

            megsToConsider.AddRange(eawMegs);
            if (eawPatch is not null)
                megsToConsider.Add(eawPatch);
            if (eawPatch2 is not null)
                megsToConsider.Add(eawPatch2);
            if (eaw64Patch is not null)
                megsToConsider.Add(eaw64Patch);
        }

        var focOrModMegs = LoadMegArchivesFromXml(".");
        var focPatch = LoadMegArchive("Data\\Patch.meg");
        var focPatch2 = LoadMegArchive("Data\\Patch2.meg");
        var foc64Patch = LoadMegArchive("Data\\64Patch.meg");

        megsToConsider.AddRange(focOrModMegs);
        if (focPatch is not null)
            megsToConsider.Add(focPatch);
        if (focPatch2 is not null)
            megsToConsider.Add(focPatch2);
        if (foc64Patch is not null)
            megsToConsider.Add(foc64Patch);

        AddMegFiles(megsToConsider);
    }


    protected override T? RepositoryFileLookup<T>(string filePath, Func<string, ActionResult<T>> pathAction, Func<string, ActionResult<T>> megAction, bool megFileOnly, T? defaultValue = default) where T: default
    {
        if (!megFileOnly)
        {
            foreach (var modPath in ModPaths)
            {
                var modFilePath = FileSystem.Path.Combine(modPath, filePath);

                var result = pathAction(modFilePath);
                if (result.ShallReturn)
                    return result.Result;
            }

            {
                var normalFilePath = FileSystem.Path.Combine(GameDirectory, filePath);
                var result = pathAction(normalFilePath);
                if (result.ShallReturn)
                    return result.Result;
            }
            
        }

        if (MasterMegArchive is not null)
        {
            var result = megAction(filePath);
            if (result.ShallReturn)
                return result.Result;
        }

        if (!megFileOnly)
        {
            foreach (var fallbackPath in FallbackPaths)
            {
                var fallbackFilePath = FileSystem.Path.Combine(fallbackPath, filePath);

                var result = pathAction(fallbackFilePath);
                if (result.ShallReturn)
                    return result.Result;
            }
        }

        return defaultValue;
    }
}