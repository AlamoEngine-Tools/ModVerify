using System;
using System.Linq;
using AET.ModVerify.Reporting.Suppressions.Json;

namespace AET.ModVerify.Reporting.Suppressions;

/// <summary>Represents a filter that suppresses verification errors matching a combination of error identifier, verifier, and asset.</summary>
public sealed class SuppressionFilter : IEquatable<SuppressionFilter>
{
    /// <summary>Gets the error identifier to match, or <see langword="null"/> to match any identifier.</summary>
    public string? Id { get; }

    /// <summary>Gets the verifier name to match, or <see langword="null"/> to match any verifier.</summary>
    public string? Verifier { get; }

    /// <summary>Gets the asset to match, or <see langword="null"/> to match any asset.</summary>
    public string? Asset { get; }

    /// <summary>Gets a value that indicates whether the filter is disabled because no matching criterion is set.</summary>
    /// <value><see langword="true"/> if none of <see cref="Id"/>, <see cref="Verifier"/>, and <see cref="Asset"/> is set; otherwise, <see langword="false"/>.</value>
    public bool IsDisabled => Id == null && Verifier == null && Asset == null;

    /// <summary>Initializes a new instance of the <see cref="SuppressionFilter"/> class.</summary>
    /// <param name="id">The error identifier to match, or <see langword="null"/> to match any identifier.</param>
    /// <param name="verifier">The verifier name to match, or <see langword="null"/> to match any verifier.</param>
    /// <param name="asset">The asset to match, or <see langword="null"/> to match any asset.</param>
    public SuppressionFilter(string? id, string? verifier, string? asset)
    {
        Id = id;
        Verifier = verifier;
        Asset = asset;
    }

    internal SuppressionFilter(JsonSuppressionFilter filter)
    {
        Id = filter.Id;
        Verifier = filter.Verifier;
        Asset = filter.Asset;
    }

    /// <summary>Determines whether this filter suppresses the specified error.</summary>
    /// <param name="error">The verification error to test.</param>
    /// <returns><see langword="true"/> if the filter suppresses <paramref name="error"/>; otherwise, <see langword="false"/>.</returns>
    public bool Suppresses(VerificationError error)
    {
        var suppresses = false;

        if (Id is not null)
        {
            if (Id.Equals(error.Id))
                suppresses = true;
            else
                return false;
        }

        if (Verifier is not null)
        {
            if (error.VerifierChain.Any(x => x.Name.Equals(Verifier)))
                suppresses = true;
            else
                return false;
        }

        if (Asset is not null)
        {
            if (error.Asset.Equals(Asset))
                suppresses = true;
            else
                return false;
        }

        return suppresses;
    }

    /// <inheritdoc />
    public bool Equals(SuppressionFilter? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        if (Id != other.Id)
            return false;
        if (Verifier != other.Verifier)
            return false;
        return Asset == other.Asset;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is SuppressionFilter other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Id);
        hashCode.Add(Verifier);
        hashCode.Add(Asset);
        return hashCode.ToHashCode();
    }
}