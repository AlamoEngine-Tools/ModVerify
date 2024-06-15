using System;
using System.Linq;
using System.Threading;
using AnakinRaW.CommonUtilities.Collections;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.DataTypes;

namespace AET.ModVerify.Steps;

public sealed class DuplicateFinderStep(
    IGameDatabase gameDatabase, 
    VerificationSettings settings, 
    IServiceProvider serviceProvider)
    : GameVerificationStep(gameDatabase, settings, serviceProvider)
{
    public const string DuplicateFound = "DUP00";

    protected override string LogFileName => "Duplicates";

    public override string Name => "Duplicate Definitions";

    protected override void RunVerification(CancellationToken token)
    {
        CheckDatabaseForDuplicates(Database.GameObjects, "GameObject");
        CheckDatabaseForDuplicates(Database.SfxEvents, "SFXEvent");
    }

    private void CheckDatabaseForDuplicates<T>(IXmlDatabase<T> database, string context) where T : XmlObject
    {
        foreach (var key in database.EntryKeys)
        {
            var entries = database.GetEntries(key);
            if (entries.Count > 1)
                AddError(VerificationError.Create(DuplicateFound, CreateDuplicateErrorMessage(context, entries)));
        }
    }

    private string CreateDuplicateErrorMessage<T>(string context, ReadOnlyFrugalList<T> entries) where T : XmlObject
    {
        var firstEntry = entries.First();

        var message = $"{context} '{firstEntry.Name}' ({firstEntry.Crc32}) has duplicate definitions: ";

        foreach (var entry in entries) 
            message += $"['{entry.Name}' in {entry.Location.XmlFile}:{entry.Location.Line}] ";

        return message.TrimEnd();
    }
}