using System;
using System.Collections.Generic;
using System.Linq;
using AET.ModVerify.Reporting.Json;

namespace AET.ModVerify.Reporting;

public sealed class SuppressionFilter : IEquatable<SuppressionFilter>
{
    private static readonly AssetsEqualityComparer AssetComparer = AssetsEqualityComparer.Instance;

    private readonly HashSet<string>? _assets;

    public string? Id { get; }

    public string? Verifier { get; }

    public IReadOnlyCollection<string>? Assets { get; }

    public SuppressionFilter(string? id, string? verifier, ICollection<string>? assets)
    {
        Id = id;
        Verifier = verifier;
        if (assets is not null) 
            _assets = new HashSet<string>(assets);
        Assets = _assets?.ToList() ?? null;
    }

    internal SuppressionFilter(JsonSuppressionFilter filter)
    {
        Id = filter.Id;
        Verifier = filter.Verifier;

        if (filter.Assets is not null)
            _assets = new HashSet<string>(filter.Assets);
        Assets = _assets?.ToList() ?? null;
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
            if (Verifier.Equals(error.Verifier))
                suppresses = true;
            else
                return false;
        }

        if (_assets is not null)
        {
            if (_assets.Count != error.AffectedAssets.Count)
                return false;

            if (!_assets.SetEquals(error.AffectedAssets))
                return false;

            suppresses = true;
        }

        return suppresses;
    }

    public bool IsDisabled()
    {
        return Id == null && Verifier == null && (Assets == null || !Assets.Any());
    }

    public int Specificity()
    {
        var specificity = 0;
        if (Id != null) 
            specificity++;
        if (Verifier != null)
            specificity++;
        if (Assets != null && Assets.Any())
            specificity++;
        return specificity;
    }

    public bool IsSupersededBy(SuppressionFilter other)
    {
        if (Id != null && other.Id != null && other.Id != Id)
            return false;

        if (Verifier != null && other.Verifier != null && other.Verifier != Verifier)
            return false;

        if (Assets != null && other.Assets != null && !AssetComparer.Equals(_assets, other._assets))
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

        return AssetComparer.Equals(_assets, other._assets);
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
        if (_assets is not null)
            hashCode.Add(_assets, AssetComparer);
        else
            hashCode.Add((HashSet<string>)null!);
        return hashCode.ToHashCode();
    }
}