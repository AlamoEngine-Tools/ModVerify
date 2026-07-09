using System;

namespace AET.ModVerify.Reporting.Baseline;

/// <summary>Associates a verification baseline with a unique identifier and the source it was loaded from.</summary>
public sealed record IdentifiedBaseline
{
    /// <summary>Gets the unique identifier of the baseline within its collection.</summary>
    public string Identifier { get; }

    /// <summary>Gets the verification baseline.</summary>
    public VerificationBaseline Baseline { get; }

    /// <summary>Gets the source the baseline was loaded from.</summary>
    public BaselineSource Source { get; }

    /// <summary>Initializes a new instance of the <see cref="IdentifiedBaseline"/> class.</summary>
    /// <param name="identifier">The unique identifier of the baseline within its collection.</param>
    /// <param name="baseline">The verification baseline.</param>
    /// <param name="source">One of the enumeration values that specifies the source the baseline was loaded from.</param>
    /// <exception cref="ArgumentException"><paramref name="identifier"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="baseline"/> is <see langword="null"/>.</exception>
    public IdentifiedBaseline(string identifier, VerificationBaseline baseline, BaselineSource source)
    {
        if (string.IsNullOrEmpty(identifier))
            throw new ArgumentException("Identifier must be non-empty.", nameof(identifier));
        Identifier = identifier;
        Baseline = baseline ?? throw new ArgumentNullException(nameof(baseline));
        Source = source;
    }
}
