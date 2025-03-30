using System;
using PG.Commons.Hashing;

namespace AET.ModVerify.Verifiers;

public interface IAlreadyVerifiedCache
{
    public bool TryAddEntry(string entry);
    public bool TryAddEntry(ReadOnlySpan<char> entry);
    public bool TryAddEntry(Crc32 checksum);
}