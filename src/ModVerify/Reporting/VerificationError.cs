using System;
using System.Collections.Generic;
using System.Linq;
using AET.ModVerify.Reporting.Json;
using AnakinRaW.CommonUtilities;

namespace AET.ModVerify.Reporting;

public sealed class VerificationError : IEquatable<VerificationError>
{
    private static readonly AssetsEqualityComparer AssetComparer = AssetsEqualityComparer.Instance;

    private readonly HashSet<string> _assets;

    public string Id { get; }

    public string Message { get; }

    public IReadOnlyList<string> VerifierChain { get; }

    public IReadOnlyCollection<string> AffectedAssets { get; }

    public VerificationSeverity Severity { get; }

    public VerificationError(string id, string message, IReadOnlyList<string> verifiers, IEnumerable<string> affectedAssets,
        VerificationSeverity severity)
    {
        if (affectedAssets == null)
            throw new ArgumentNullException(nameof(affectedAssets));
        ThrowHelper.ThrowIfNullOrEmpty(id);
        Id = id;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        VerifierChain = verifiers;
        Severity = severity;
        _assets = [..affectedAssets];
        AffectedAssets = _assets.ToList();
    }

    internal VerificationError(JsonVerificationError error)
    {
        Id = error.Id;
        Message = error.Message;
        VerifierChain = error.VerifierChain;
        _assets = [..error.Assets];
        AffectedAssets = _assets.ToList();
    }

    public static VerificationError Create(
        IReadOnlyList<IGameVerifier> verifiers,
        string id,
        string message,
        VerificationSeverity severity,
        IEnumerable<string> assets)
    {
        return new VerificationError(id, message, verifiers.Select(x => x.Name).ToList(), assets, severity);
    }

    public static VerificationError Create(
        IReadOnlyList<IGameVerifier> verifiers,
        string id,
        string message,
        VerificationSeverity severity,
        params string[] assets)
    {
        return new VerificationError(id, message, verifiers.Select(x => x.Name).ToList(), assets, severity);
    }

    public bool Equals(VerificationError? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        if (!Id.Equals(other.Id))
            return false;

        return AssetComparer.Equals(_assets, other._assets);
    }

    public override bool Equals(object? obj)
    {
        return obj is VerificationError other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Id);
        hashCode.Add(_assets, AssetComparer);
        return hashCode.ToHashCode();
    }

    public override string ToString()
    {
        return $"[{Severity}] [{string.Join(" --> ", VerifierChain)}] {Id}: Message={Message}; Affected Assets=[{string.Join(",", AffectedAssets)}];";
    }
}