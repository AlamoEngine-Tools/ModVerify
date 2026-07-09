namespace AET.ModVerify.Verifiers.Caching;

/// <summary>Represents the result of a lookup in an <see cref="IAlreadyVerifiedCache"/>.</summary>
public readonly struct VerifiedCacheEntry
{
    /// <summary>Gets a value that indicates whether the asset has already been verified.</summary>
    /// <value><see langword="true"/> if the asset has already been verified; otherwise, <see langword="false"/>.</value>
    public bool AlreadyVerified { get; }

    /// <summary>Gets a value that indicates whether the asset was found during its earlier verification.</summary>
    /// <value><see langword="true"/> if the asset was found; otherwise, <see langword="false"/>.</value>
    public bool AssetExists { get; }

    /// <summary>Initializes a new instance of the <see cref="VerifiedCacheEntry"/> struct.</summary>
    /// <param name="alreadyVerified"><see langword="true"/> if the asset has already been verified; otherwise, <see langword="false"/>.</param>
    /// <param name="assetExists"><see langword="true"/> if the asset was found; otherwise, <see langword="false"/>.</param>
    public VerifiedCacheEntry(bool alreadyVerified, bool assetExists)
    {
        AlreadyVerified = alreadyVerified;
        AssetExists = assetExists;
    }
}