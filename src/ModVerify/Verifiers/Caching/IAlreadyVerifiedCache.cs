namespace AET.ModVerify.Verifiers.Caching;

/// <summary>Provides a cache that tracks which assets have already been verified within a single verification run.</summary>
public interface IAlreadyVerifiedCache
{
    /// <summary>Adds an entry to the cache if it is not already present.</summary>
    /// <param name="entry">The identifier of the asset, such as a file name.</param>
    /// <param name="assetExists">
    /// When this method returns, contains a value indicating whether the asset was found.
    /// <see langword="true"/> if the asset was found; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the entry was added;
    /// <see langword="false"/> if an entry for <paramref name="entry"/> already existed.
    /// </returns>
    bool TryAddEntry(string entry, bool assetExists);

    /// <summary>Gets the cache entry for the specified asset.</summary>
    /// <param name="entry">The identifier of the asset, such as a file name.</param>
    /// <returns>
    /// The cache entry, or a default entry whose <see cref="VerifiedCacheEntry.AlreadyVerified"/> is <see langword="false"/> 
    /// if the asset has not been verified.
    /// </returns>
    VerifiedCacheEntry GetEntry(string entry);
}