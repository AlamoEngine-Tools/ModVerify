using System.Collections.Generic;
using PG.Commons.Hashing;

namespace AET.ModVerify.Verifiers.Commons;

public interface IDuplicateVerificationContext
{
    string SourceName { get; }
    IEnumerable<Crc32> GetCrcs();
    bool HasDuplicates(Crc32 crc, out string entryNames, out IEnumerable<string> duplicateContext, out string errorMessage);
}