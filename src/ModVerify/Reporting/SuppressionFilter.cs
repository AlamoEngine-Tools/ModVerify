using System;
using System.Linq;
using AET.ModVerify.Reporting.Json;

namespace AET.ModVerify.Reporting;

public sealed class SuppressionFilter : IEquatable<SuppressionFilter>
{
    public string? Id { get; }

    public string? Verifier { get; }

    public string? Asset { get; }

    public bool IsDisabled => Id == null && Verifier == null && Asset == null;

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
            if (error.VerifierChain.Contains(Verifier))
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

    public int Specificity()
    {
        var specificity = 0;
        if (Id != null) 
            specificity++;
        if (Verifier != null)
            specificity++;
        if (Asset != null)
            specificity++;
        return specificity;
    }

    public bool IsSupersededBy(SuppressionFilter other)
    {
        if (Id != null && other.Id != null && other.Id != Id)
            return false;

        if (Verifier != null && other.Verifier != null && other.Verifier != Verifier)
            return false;

        if (Asset != null && other.Asset != null)
            return false;

        return other.Specificity() < Specificity();
    }

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

    public override bool Equals(object? obj)
    {
        return obj is SuppressionFilter other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Id);
        hashCode.Add(Verifier);
        hashCode.Add(Asset);
        return hashCode.ToHashCode();
    }
}