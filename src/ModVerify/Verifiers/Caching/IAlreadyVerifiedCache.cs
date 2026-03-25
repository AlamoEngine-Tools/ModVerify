namespace AET.ModVerify.Verifiers.Caching;

public interface IAlreadyVerifiedCache
{
    bool TryAddEntry(string entry, bool assetExists);

    VerifiedCacheEntry GetEntry(string entry);
}