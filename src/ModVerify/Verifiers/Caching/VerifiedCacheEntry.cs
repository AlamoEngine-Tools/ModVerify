namespace AET.ModVerify.Verifiers.Caching;

public readonly struct VerifiedCacheEntry
{
    public bool AlreadyVerified { get; }

    public bool AssetExists { get; }

    public VerifiedCacheEntry(bool alreadyVerified, bool assetExists)
    {
        AlreadyVerified = alreadyVerified;
        AssetExists = assetExists;
    }
}