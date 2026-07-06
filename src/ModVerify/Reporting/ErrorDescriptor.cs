using System;
using System.Collections.Generic;
using AET.ModVerify.Verifiers;
using AnakinRaW.CommonUtilities;

namespace AET.ModVerify.Reporting;

/// <summary>Represents the static definition of a kind of verification finding: its code, name, severity, and category.</summary>
/// <remarks>
/// One <see cref="Id"/> may be shared by several descriptors (for example, the many kinds of "file not found" all use
/// <c>FILE00</c>). The <see cref="Id"/> is what baselines and suppressions match on, while a descriptor identifies one
/// concrete sub-kind with a single <see cref="Severity"/>; a finding that warrants a different severity is a different
/// descriptor. Descriptor identity is therefore the <see cref="Name"/>, not the <see cref="Id"/>. The human-readable
/// message is supplied per occurrence by the catalog factory methods, not stored on the descriptor.
/// </remarks>
public sealed class ErrorDescriptor : IEquatable<ErrorDescriptor>
{
    /// <summary>Gets the error code of the descriptor.</summary>
    /// <remarks>
    /// The value is allowed to be shared across descriptors.
    /// </remarks>
    /// <value>The error code, such as <c>FILE00</c>.</value>
    public string Id { get; }

    /// <summary>Gets the unique symbolic name of the descriptor.</summary>
    /// <value>The unique symbolic name, such as <c>AudioFileNotFound</c>.</value>
    public string Name { get; }

    /// <summary>Gets the severity of the error.</summary>
    public VerificationSeverity Severity { get; }

    /// <summary>Gets the grouping category.</summary>
    /// <value>The grouping category, such as <c>Audio</c>.</value>
    public string Category { get; }

    /// <summary>Initializes a new instance of the <see cref="ErrorDescriptor"/> class.</summary>
    /// <param name="id">The error code.</param>
    /// <param name="name">The unique symbolic name.</param>
    /// <param name="severity">One of the enumeration values that specifies the severity of the finding.</param>
    /// <param name="category">The grouping category used for the rule reference.</param>
    /// <exception cref="ArgumentNullException"><paramref name="id"/>, <paramref name="name"/>, or <paramref name="category"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="id"/>, <paramref name="name"/>, or <paramref name="category"/> is empty.</exception>
    public ErrorDescriptor(string id, string name, VerificationSeverity severity, string category)
    {
        ThrowHelper.ThrowIfNullOrEmpty(id);
        ThrowHelper.ThrowIfNullOrEmpty(name);
        ThrowHelper.ThrowIfNullOrEmpty(category);

        Id = id;
        Name = name;
        Severity = severity;
        Category = category;
    }

    /// <summary>
    /// Creates a <see cref="VerificationError"/> instance for this descriptor with the given message, asset, and context.
    /// </summary>
    /// <param name="verifier">The game verifier info.</param>
    /// <param name="message">The error message.</param>
    /// <param name="asset">The asset associated with the error.</param>
    /// <param name="context">The context for the error.</param>
    /// <returns>The created verification error instance.</returns>
    public VerificationError Create(IGameVerifierInfo verifier, string message, string asset, IEnumerable<string> context)
    {
        return new VerificationError(Id, message, verifier, context, asset, Severity);
    }

    /// <inheritdoc/>
    public bool Equals(ErrorDescriptor? other)
    {
        return other is not null && Name.Equals(other.Name, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is ErrorDescriptor other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Id} ({Name})";
    }
}
