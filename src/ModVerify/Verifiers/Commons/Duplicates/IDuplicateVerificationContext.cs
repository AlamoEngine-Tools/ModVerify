using System.Collections.Generic;
using PG.Commons.Hashing;

namespace AET.ModVerify.Verifiers.Commons;

/// <summary>Provides the data a duplicate verifier needs to detect duplicate entries within a source.</summary>
public interface IDuplicateVerificationContext
{
    /// <summary>Gets the name of the source being checked for duplicates.</summary>
    string SourceName { get; }

    /// <summary>Gets the CRC-32 checksums of the entries in the source.</summary>
    /// <returns>The checksums of all entries.</returns>
    IEnumerable<Crc32> GetCrcs();

    /// <summary>Determines whether the entry with the specified checksum has duplicates.</summary>
    /// <param name="crc">The checksum of the entry to check.</param>
    /// <param name="entryNames">When this method returns, contains the name of the entry. This parameter is treated as uninitialized.</param>
    /// <param name="duplicateContext">When this method returns, contains the context entries describing the duplicates. This parameter is treated as uninitialized.</param>
    /// <param name="errorMessage">When this method returns, contains the message describing the duplication. This parameter is treated as uninitialized.</param>
    /// <returns><see langword="true"/> if the entry has duplicates; otherwise, <see langword="false"/>.</returns>
    bool HasDuplicates(Crc32 crc, out string entryNames, out IEnumerable<string> duplicateContext, out string errorMessage);
}