using System;
using System.Collections.Generic;
using System.Linq;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Files.XML.Data;

namespace AET.ModVerify.Verifiers.Commons;

internal sealed class NamedXmlObjectDuplicateVerificationContext<T>(string databaseName, IGameManager<T> gameManager)
    : IDuplicateVerificationContext 
    where T : NamedXmlObject
{
    public string SourceName => databaseName;

    public IEnumerable<Crc32> GetCrcs() => gameManager.EntryKeys;

    public bool HasDuplicates(Crc32 crc, out string entryNames, out IEnumerable<string> duplicateContext, out string errorMessage)
    {
        var entries = gameManager.GetEntries(crc);
        if (entries.Count > 1)
        {
            var firstEntry = entries.First();
            entryNames = firstEntry.Name;
            duplicateContext = entries.Select(x => $"'{x.Name}' - {x.Location}");
            var message = $"{SourceName} '{firstEntry.Name}' ({firstEntry.Crc32}) has duplicate definitions: ";
            message = entries.Aggregate(message, (current, entry) => current + $"['{entry.Name}' in {entry.Location.XmlFile}:{entry.Location.Line}] ");
            errorMessage = message.TrimEnd();
            return true;
        }

        entryNames = string.Empty;
        duplicateContext = Array.Empty<string>();
        errorMessage = string.Empty;
        return false;
    }
}