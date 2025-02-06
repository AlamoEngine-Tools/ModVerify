using System;
using System.Collections.Generic;
using System.Linq;
using PG.StarWarsGame.Engine.Database.ErrorReporting;
using PG.StarWarsGame.Engine.Utilities;
using PG.StarWarsGame.Files.MEG.Files;

namespace PG.StarWarsGame.Engine.IO.Repositories;

// EaW file lookup works slightly different!
internal class FocGameRepository : GameRepository
{
    public override GameEngineType EngineType => GameEngineType.Foc;

    public FocGameRepository(GameLocations gameLocations, DatabaseErrorReporterWrapper errorReporter, IServiceProvider serviceProvider)
        : base(gameLocations, errorReporter, serviceProvider)
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
    
    protected internal override FileFoundInfo FindFile(ReadOnlySpan<char> filePath, ref ValueStringBuilder pathStringBuilder, bool megFileOnly = false)
    {
        if (!megFileOnly)
        {
            var fileFoundInfo = FileFromAltExists(filePath, ModPaths, ref pathStringBuilder);
            if (fileFoundInfo.FileFound)
                return fileFoundInfo;

            fileFoundInfo = FindFileCore(filePath, ref pathStringBuilder);
            if (fileFoundInfo.FileFound)
                return fileFoundInfo;
        }


        if (MasterMegArchive is not null)
        {
            var fileFoundInfo = GetFileInfoFromMasterMeg(filePath);
            if (fileFoundInfo.InMeg)
                return fileFoundInfo;
        }

        if (!megFileOnly)
            return FileFromAltExists(filePath, FallbackPaths, ref pathStringBuilder);

        return default;
    }
}