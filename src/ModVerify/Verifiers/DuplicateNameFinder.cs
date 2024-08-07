﻿using System;
using System.Linq;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AnakinRaW.CommonUtilities.Collections;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.DataTypes;

namespace AET.ModVerify.Verifiers;

public sealed class DuplicateNameFinder(
    IGameDatabase gameDatabase, 
    GameVerifySettings settings, 
    IServiceProvider serviceProvider)
    : GameVerifierBase(gameDatabase, settings, serviceProvider)
{
    public override string FriendlyName => "Duplicate Definitions";

    protected override void RunVerification(CancellationToken token)
    {
        CheckDatabaseForDuplicates(Database.GameObjects, "GameObject");
        CheckDatabaseForDuplicates(Database.SfxEvents, "SFXEvent");
    }

    private void CheckDatabaseForDuplicates<T>(IXmlDatabase<T> database, string databaseName) where T : XmlObject
    {
        foreach (var key in database.EntryKeys)
        {
            var entries = database.GetEntries(key);
            if (entries.Count > 1)
            {
                var entryNames = entries.Select(x => x.Name);
                AddError(VerificationError.Create(
                    this,
                    VerifierErrorCodes.DuplicateFound,
                    CreateDuplicateErrorMessage(databaseName, entries), 
                    VerificationSeverity.Warning,
                    entryNames));
            }
        }
    }

    private string CreateDuplicateErrorMessage<T>(string databaseName, ReadOnlyFrugalList<T> entries) where T : XmlObject
    {
        var firstEntry = entries.First();

        var message = $"{databaseName} '{firstEntry.Name}' ({firstEntry.Crc32}) has duplicate definitions: ";

        foreach (var entry in entries) 
            message += $"['{entry.Name}' in {entry.Location.XmlFile}:{entry.Location.Line}] ";

        return message.TrimEnd();
    }
}