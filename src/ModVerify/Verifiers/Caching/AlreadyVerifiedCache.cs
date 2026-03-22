using System.Collections.Generic;

namespace AET.ModVerify.Verifiers.Caching;

internal sealed class AlreadyVerifiedCache : IAlreadyVerifiedCache
{
    private readonly Dictionary<string, bool> _cachedEntries = new();
    
    public bool TryAddEntry(string entry, bool assetExists)
    {
        var upper = entry.ToUpperInvariant();

#if NETSTANDARD2_1 || NET
        return _cachedEntries.TryAdd(upper, assetExists);
#else
        var alreadyVerified = _cachedEntries.ContainsKey(upper);
        if (alreadyVerified)
            return false;

        _cachedEntries[upper] = assetExists;
        return true;
#endif
    }

    public VerifiedCacheEntry GetEntry(string entry)
    {
        var upper = entry.ToUpperInvariant();
        var alreadyVerified = _cachedEntries.TryGetValue(upper, out var exists);
        return alreadyVerified ? new VerifiedCacheEntry(true, exists) : default;
    }
}