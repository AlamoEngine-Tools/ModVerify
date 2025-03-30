using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Verifiers;

internal sealed class AlreadyVerifiedCache(IServiceProvider serviceProvider) : IAlreadyVerifiedCache
{
    private readonly ICrc32HashingService _crc32Hashing = serviceProvider.GetRequiredService<ICrc32HashingService>();
    private readonly ConcurrentDictionary<Crc32, byte> _cachedChecksums = new();

    public bool TryAddEntry(string entry)
    {
        return TryAddEntry(entry.AsSpan());
    }

    public bool TryAddEntry(ReadOnlySpan<char> entry)
    {
        return TryAddEntry(_crc32Hashing.GetCrc32Upper(entry, PGConstants.DefaultPGEncoding));
    }

    public bool TryAddEntry(Crc32 checksum)
    {
        return _cachedChecksums.TryAdd(checksum, 0);
    }
}