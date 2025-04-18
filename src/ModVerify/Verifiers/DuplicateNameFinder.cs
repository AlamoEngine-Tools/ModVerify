﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AnakinRaW.CommonUtilities.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Xml;
using PG.StarWarsGame.Files.MTD.Data;
using PG.StarWarsGame.Files.MTD.Files;

namespace AET.ModVerify.Verifiers;

public sealed class DuplicateNameFinder(
    IStarWarsGameEngine gameEngine, 
    GameVerifySettings settings, 
    IServiceProvider serviceProvider)
    : GameVerifier(null, gameEngine, settings, serviceProvider)
{
    public override string FriendlyName => "Duplicates";

    public override void Verify(CancellationToken token)
    {
        CheckXmlObjectsForDuplicates("GameObject", GameEngine.GameObjectTypeManager);
        CheckXmlObjectsForDuplicates("SFXEvent", GameEngine.SfxGameManager);

        if (GameEngine.GuiDialogManager.MtdFile is not null)
            CheckMtdForDuplicates(GameEngine.GuiDialogManager.MtdFile);

        if (GameEngine.CommandBar.MegaTextureFile is not null)
        {
            if (!GameEngine.CommandBar.MegaTextureFile.FilePath.Equals(GameEngine.GuiDialogManager.MtdFile?.FileName))
                CheckMtdForDuplicates(GameEngine.CommandBar.MegaTextureFile);
        }
    } 
    
    private void CheckForDuplicateCrcEntries<TSource, TEntry>(
        string sourceName,
        TSource source, 
        Func<TSource, IEnumerable<Crc32>> crcSelector, 
        Func<TSource, Crc32, ReadOnlyFrugalList<TEntry>> entrySelector, 
        Func<TEntry, string> entryToStringSelector,
        Func<ReadOnlyFrugalList<TEntry>, IEnumerable<string>> contextSelector,
        Func<ReadOnlyFrugalList<TEntry>, string, string> errorMessageCreator)
    {
        foreach (var crc32 in crcSelector(source))
        {
            var entries = entrySelector(source, crc32);
            if (entries.Count > 1)
            {
                var entryNames = entryToStringSelector(entries.First());
                var context = contextSelector(entries);
                AddError(VerificationError.Create(
                    VerifierChain,
                    VerifierErrorCodes.DuplicateFound,
                    errorMessageCreator(entries, sourceName),
                    VerificationSeverity.Error,
                    context,
                    entryNames));
            }
        }
    }

    private void CheckMtdForDuplicates(IMtdFile mtdFile)
    {
        CheckForDuplicateCrcEntries(
            mtdFile.FileName,
            mtdFile,
            mtd => mtd.Content.Select(x => x.Crc32),
            (mtd, crc32) => mtd.Content.EntriesWithCrc(crc32), 
            entry => entry.FileName,
            entries => entries.Select(x => $"'{x.FileName}' (CRC: {x.Crc32})"),
            CreateDuplicateMtdErrorMessage);
    }

    private void CheckXmlObjectsForDuplicates<T>(string databaseName, IGameManager<T> gameManager) where T : NamedXmlObject
    {
        CheckForDuplicateCrcEntries(
            databaseName,
            gameManager,
            manager => manager.EntryKeys,
            (manager, crc32) => manager.GetEntries(crc32), 
            entry => entry.Name,
            entries => entries.Select(x => $"'{x.Name}' - {x.Location}"),
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