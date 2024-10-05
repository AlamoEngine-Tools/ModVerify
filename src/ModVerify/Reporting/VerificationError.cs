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

    public string Verifier { get; }

    public IReadOnlyCollection<string> AffectedAssets { get; }

    public VerificationSeverity Severity { get; }

    public VerificationError(string id, string message, string verifier, IEnumerable<string> affectedAssets, VerificationSeverity severity)
    {
        if (affectedAssets == null) 
            throw new ArgumentNullException(nameof(affectedAssets));
        ThrowHelper.ThrowIfNullOrEmpty(id);
        Id = id;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Verifier = verifier;
        Severity = severity;
        _assets = new HashSet<string>(affectedAssets);
        AffectedAssets = _assets.ToList();
    }

    internal VerificationError(JsonVerificationError error)
    {
        Id = error.Id;
        Message = error.Message;
        Verifier = error.Verifier;
        _assets = new HashSet<string>(error.Assets);
        AffectedAssets = _assets.ToList();
    }

    public static VerificationError Create(
        IGameVerifier verifier,
        string id,
        string message,
        VerificationSeverity severity, 
        IEnumerable<string> assets)
    {
        return new VerificationError(id, message, verifier.Name, assets, severity);
    }


    public static VerificationError Create(
        IGameVerifier verifier,
        string id, 
        string message, 
        VerificationSeverity severity, 
        params string[] assets)
    {
        return new VerificationError(id, message, verifier.Name, assets, severity);
    }

    public static VerificationError Create(
        IGameVerifier verifier, 
        string id, 
        string message,
        VerificationSeverity severity)
    {
        return Create(verifier, id, message, severity, []);
    }

    internal static VerificationError Create(
        IGameVerifier verifier, 
        string id, 
        Exception exception, 
        VerificationSeverity severity,
        params string[] assets)
    {
        return new VerificationError(id, exception.Message, verifier.Name, assets, severity);
    }

    internal static VerificationError Create(
        IGameVerifier verifier,
        string id,
        Exception exception,
        VerificationSeverity severity)
    {
        return Create(verifier, id, exception, severity, []);
    }


    public bool Equals(VerificationError? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        if (!Id.Equals(other.Id))
            return false;
        if (!Verifier.Equals(other.Verifier))
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
        hashCode.Add(Verifier);
        hashCode.Add(_assets, AssetComparer);
        return hashCode.ToHashCode();
    }

    public override string ToString()
    {
        return $"[{Severity}] [{Verifier}] {Id}: Message={Message}; Affected Assets=[{string.Join(",", AffectedAssets)}];";
    }
}