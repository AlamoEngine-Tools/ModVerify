using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AnakinRaW.CommonUtilities.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.Xml;
using PG.StarWarsGame.Files.MTD.Data;
using PG.StarWarsGame.Files.MTD.Files;

namespace AET.ModVerify.Verifiers;

public sealed class DuplicateNameFinder(
    IGameDatabase gameDatabase, 
    GameVerifySettings settings, 
    IServiceProvider serviceProvider)
    : GameVerifierBase(null, gameDatabase, settings, serviceProvider)
{
    public override string FriendlyName => "Duplicates";

    public override void Verify(CancellationToken token)
    {
        CheckXmlObjectsForDuplicates("GameObject", Database.GameObjectTypeManager);
        CheckXmlObjectsForDuplicates("SFXEvent", Database.SfxGameManager);

        if (Database.GuiDialogManager.MtdFile is not null)
            CheckXmlObjectsForDuplicates(Database.GuiDialogManager.MtdFile);

        if (Database.CommandBar.MegaTextureFile is not null)
        {
            if (!Database.CommandBar.MegaTextureFile.FilePath.Equals(Database.GuiDialogManager.MtdFile?.FileName))
                CheckXmlObjectsForDuplicates(Database.CommandBar.MegaTextureFile);
        }
    } 
    
    private void CheckForDuplicateCrcEntries<TSource, TEntry>(
        string sourceName,
        TSource source, 
        Func<TSource, IEnumerable<Crc32>> crcSelector, 
        Func<TSource, Crc32, ReadOnlyFrugalList<TEntry>> entrySelector, 
        Func<ReadOnlyFrugalList<TEntry>, IEnumerable<string>> entryToStringSelector,
        Func<ReadOnlyFrugalList<TEntry>, string, string> errorMessageCreator)
    {
        foreach (var crc32 in crcSelector(source))
        {
            var entries = entrySelector(source, crc32);
            if (entries.Count > 1)
            {
                var entryNames = entryToStringSelector(entries);
                AddError(VerificationError.Create(
                    VerifierChain,
                    VerifierErrorCodes.DuplicateFound,
                    errorMessageCreator(entries, sourceName),
                    VerificationSeverity.Error,
                    entryNames));
            }
        }
    }

    private void CheckXmlObjectsForDuplicates(IMtdFile mtdFile)
    {
        CheckForDuplicateCrcEntries(
            mtdFile.FileName,
            mtdFile,
            mtd => mtd.Content.Select(x => x.Crc32),
            (mtd, crc32) => mtd.Content.EntriesWithCrc(crc32),
            list => list.Select(x => x.FileName),
            CreateDuplicateMtdErrorMessage);
    }

    private void CheckXmlObjectsForDuplicates<T>(string databaseName, IGameManager<T> gameManager) where T : NamedXmlObject
    {
        CheckForDuplicateCrcEntries(
            databaseName,
            gameManager,
            manager => manager.EntryKeys,
            (manager, crc32) => manager.GetEntries(crc32), 
            list => list.Select(x => x.Name),
            CreateDuplicateXmlErrorMessage);
    }

    private static string CreateDuplicateMtdErrorMessage(ReadOnlyFrugalList<MegaTextureFileIndex> entries, string fileName)
    {
        var firstEntry = entries.First();
        return $"MTD File '{fileName}' has duplicate definitions for CRC ({firstEntry}): {string.Join(",", entries.Select(x => x.FileName))}";
    }

    private static string CreateDuplicateXmlErrorMessage<T>(ReadOnlyFrugalList<T> entries, string databaseName) where T : NamedXmlObject
    {
        var firstEntry = entries.First();
        var message = $"{databaseName} '{firstEntry.Name}' ({firstEntry.Crc32}) has duplicate definitions: ";
        foreach (var entry in entries) 
            message += $"['{entry.Name}' in {entry.Location.XmlFile}:{entry.Location.Line}] ";
        return message.TrimEnd();
    }
}