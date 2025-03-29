using System;
using System.Collections.Concurrent;

namespace AET.ModVerify.Verifiers;

internal sealed class AlreadyVerifiedCache
{
    internal static readonly AlreadyVerifiedCache Instance = new();

    private readonly ConcurrentDictionary<string, byte> _cachedModels = new(StringComparer.OrdinalIgnoreCase);

    private AlreadyVerifiedCache()
    {
    }

    public bool TryAddModel(string fileName)
    {
        return _cachedModels.TryAdd(fileName, 0);
    }
}