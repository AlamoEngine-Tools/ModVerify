using System;
using System.Collections.Generic;
using System.Linq;
using AET.ModVerify.Reporting.Json;
using AET.ModVerify.Verifiers;
using AnakinRaW.CommonUtilities;

namespace AET.ModVerify.Reporting;

/// <summary>
/// Represents a verification error that occurred during the verification process.
/// </summary>
public sealed class VerificationError : IEquatable<VerificationError>
{
    private static readonly VerificationErrorContextEqualityComparer ContextComparer = VerificationErrorContextEqualityComparer.Instance;

    private readonly HashSet<string> _contextEntries;

    /// <summary>
    /// Gets the unique identifier of the verification error.
    /// </summary>
    /// <remarks>
    /// This identifier is used to categorize and identify specific types of errors.
    /// </remarks>
    public string Id { get; }

    /// <summary>
    /// Gets the descriptive message associated with the verification error.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the chain of verifiers that led to the occurrence of this error.
    /// </summary>
    public IReadOnlyList<IGameVerifierInfo> VerifierChain { get; }

    /// <summary>
    /// Gets the context entries that provide additional information about the error.
    /// </summary>
    public IReadOnlyCollection<string> ContextEntries { get; }

    /// <summary>
    /// Gets the severity level of the verification error, indicating its importance and impact on the verification process.
    /// </summary>
    public VerificationSeverity Severity { get; }

    /// <summary>
    /// Gets the asset associated with the verification error, providing information about the specific asset that caused the error.
    /// </summary>
    public string Asset { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VerificationError"/> class with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier of the verification error.</param>
    /// <param name="message">The descriptive message associated with the verification error.</param>
    /// <param name="verifier">The chain of verifiers that led to the occurrence of this error.</param>
    /// <param name="contextEntries">The context entries that provide additional information about the error.</param>
    /// <param name="asset">The asset associated with the verification error.</param>
    /// <param name="severity">The severity level of the verification error.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="id"/>,
    /// <paramref name="message"/>,
    /// <paramref name="verifier"/>,
    /// <paramref name="contextEntries"/> or
    /// <paramref name="asset"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException"><paramref name="id"/> is empty.</exception>
    public VerificationError(
        string id, 
        string message, 
        IGameVerifierInfo verifier,
        IEnumerable<string> contextEntries, 
        string asset,
        VerificationSeverity severity)
    {
        if (verifier == null)
            throw new ArgumentNullException(nameof(verifier));
        if (contextEntries == null)
            throw new ArgumentNullException(nameof(contextEntries));
        if (asset is null)
            throw new ArgumentNullException(nameof(asset));
        ThrowHelper.ThrowIfNullOrEmpty(id);

        Id = id;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        VerifierChain = verifier.VerifierChain;
        Severity = severity;
        var orderedContext = contextEntries.Distinct().ToList();
        ContextEntries = orderedContext;
        _contextEntries = [.. orderedContext];
        Asset = asset;
    }

    internal VerificationError(JsonVerificationError error)
    {
        Id = error.Id;
        Message = error.Message;
        VerifierChain = RestoreVerifierChain(error.VerifierChain);
        var orderedContext = error.ContextEntries.Distinct().ToList();
        ContextEntries = orderedContext;
        _contextEntries = [.. orderedContext];
        Asset = error.Asset;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is VerificationError other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Id);
        hashCode.Add(_contextEntries, ContextComparer);
        hashCode.Add(Asset);
        return hashCode.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"[{Severity}] [{string.Join(" --> ", VerifierChain)}] " +
               $"{Id}: Message={Message}; Asset='{Asset}'; Context=[{string.Join(",", ContextEntries)}];";
    }

    private static IReadOnlyList<IGameVerifierInfo> RestoreVerifierChain(IReadOnlyList<string>? errorVerifierChain)
    {
        if (errorVerifierChain is null)
            return [];

        var verifierChain = new List<IGameVerifierInfo>();
        IGameVerifierInfo? previousVerifier = null;

        foreach (var name in errorVerifierChain)
        {
            var verifier = new RestoredVerifierInfo
            {
                Name = name,
                Parent = previousVerifier
            };
            verifierChain.Add(verifier);
            previousVerifier = verifier;
        }

        return verifierChain;
    }
}