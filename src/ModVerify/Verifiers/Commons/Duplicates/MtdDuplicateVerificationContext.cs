using System.Collections.Generic;
using System.Linq;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.MTD.Files;

namespace AET.ModVerify.Verifiers.Commons;

internal sealed class MtdDuplicateVerificationContext(IMtdFile mtdFile) : IDuplicateVerificationContext
{
    public string SourceName => mtdFile.FileName;

    public IEnumerable<Crc32> GetCrcs() => mtdFile.Content.Select(x => x.Crc32);

    public bool HasDuplicates(Crc32 crc, out string entryNames, out IEnumerable<string> duplicateContext, out string errorMessage)
    {
        var entries = mtdFile.Content.EntriesWithCrc(crc);
        if (entries.Count > 1)
        {
            var firstEntry = entries.First();
            entryNames = firstEntry.FileName;
            duplicateContext = entries.Select(x => $"'{x.FileName}' (CRC: {x.Crc32})");
            errorMessage = $"MTD File '{SourceName}' has duplicate definitions for CRC ({firstEntry}): " +
                           $"{string.Join(",", entries.Select(x => x.FileName))}";
            return true;
        }

        entryNames = string.Empty;
        duplicateContext = [];
        errorMessage = string.Empty;
        return false;
    }
}