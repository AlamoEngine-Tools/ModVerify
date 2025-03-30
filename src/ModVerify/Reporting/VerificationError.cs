using System;
using System.Collections.Generic;
using System.Linq;
using AET.ModVerify.Reporting.Json;
using AET.ModVerify.Verifiers;
using AnakinRaW.CommonUtilities;

namespace AET.ModVerify.Reporting;

public sealed class VerificationError : IEquatable<VerificationError>
{
    private static readonly VerificationErrorContextEqualityComparer ContextComparer = VerificationErrorContextEqualityComparer.Instance;

    private readonly HashSet<string> _contextEntries;

    public string Id { get; }

    public string Message { get; }

    public IReadOnlyList<string> VerifierChain { get; }

    public IReadOnlyCollection<string> ContextEntries { get; }

    public VerificationSeverity Severity { get; }

    public string Asset { get; }

    public VerificationError(
        string id, 
        string message, 
        IReadOnlyList<string> verifiers,
        IEnumerable<string> contextEntries, 
        string asset,
        VerificationSeverity severity)
    {
        if (contextEntries == null)
            throw new ArgumentNullException(nameof(contextEntries));
        if (asset is null)
            throw new ArgumentNullException(nameof(asset));
        ThrowHelper.ThrowIfNullOrEmpty(id);

        Id = id;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        VerifierChain = verifiers;
        Severity = severity;
        _contextEntries = [.. contextEntries];
        ContextEntries = _contextEntries.ToList();
        Asset = asset;
    }

    internal VerificationError(JsonVerificationError error)
    {
        Id = error.Id;
        Message = error.Message;
        VerifierChain = error.VerifierChain;
        _contextEntries = [..error.ContextEntries];
        ContextEntries = _contextEntries.ToList();
        Asset = error.Asset;
    }

    public static VerificationError Create(
        IReadOnlyList<IGameVerifierInfo> verifiers,
        string id,
        string message,
        VerificationSeverity severity,
        IEnumerable<string> context,
        string asset)
    {
        return new VerificationError(id, message, verifiers.Select(x => x.Name).ToList(), context, asset, severity);
    }

    public static VerificationError Create(
        IReadOnlyList<IGameVerifierInfo> verifiers,
        string id,
        string message,
        VerificationSeverity severity,
        string asset)
    {
        return new VerificationError(
            id,
            message, 
            verifiers.Select(x => x.Name).ToList(),
            [], 
            asset, 
            severity);
    }

    public bool Equals(VerificationError? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        if (!Id.Equals(other.Id))
            return false;

        if (!Asset.Equals(other.Asset))
            return false;

        return ContextComparer.Equals(_contextEntries, other._contextEntries);
    }

    public override bool Equals(object? obj)
    {
        return obj is VerificationError other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Id);
        hashCode.Add(_contextEntries, ContextComparer);
        hashCode.Add(Asset);
        return hashCode.ToHashCode();
    }

    public override string ToString()
    {
        return $"[{Severity}] [{string.Join(" --> ", VerifierChain)}] {Id}: Message={Message}; Asset='{Asset}'; Context=[{string.Join(",", ContextEntries)}];";
    }
}