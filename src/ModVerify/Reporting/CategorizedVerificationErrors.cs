using System;
using System.Collections.Generic;
using AnakinRaW.CommonUtilities.Collections;

namespace AET.ModVerify.Reporting;

/// <summary>
/// Represents the errors associated with a verification result, categorized into new, persistent and resolved errors.
/// </summary>
public sealed class CategorizedVerificationErrors
{
    /// <summary>
    /// Gets an empty <see cref="CategorizedVerificationErrors"/> with no errors.
    /// </summary>
    public static readonly CategorizedVerificationErrors Empty = new(
        [],
        ReadOnlyValueListDictionary<string, VerificationError>.Empty,
        ReadOnlyValueListDictionary<string, VerificationError>.Empty);

    /// <summary>
    /// Gets a collection containing errors found in the verification process and which are not present in the processed baseline.
    /// </summary>
    public IReadOnlyCollection<VerificationError> NewErrors { get; }

    /// <summary>
    /// Gets the errors, keyed by their matching baseline identifier, that were already present in
    /// the baseline processed and were found again during the verification process.
    /// </summary>
    public IReadOnlyValueListDictionary<string, VerificationError> PersistentErrors { get; }

    /// <summary>
    /// Gets the errors, keyed by their matching baseline identifier, that were present
    /// in the processed baseline but were not found during the verification process.
    /// </summary>
    public IReadOnlyValueListDictionary<string, VerificationError> ResolvedErrors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CategorizedVerificationErrors"/> class.
    /// </summary>
    /// <param name="newErrors">The collection of new errors.</param>
    /// <param name="persistentErrors">The dictionary of persistent errors.</param>
    /// <param name="resolvedErrors">The dictionary of resolved errors.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="newErrors"/>, <paramref name="persistentErrors"/>, or <paramref name="resolvedErrors"/> is <see langword="null"/>.
    /// </exception>
    public CategorizedVerificationErrors(
        IReadOnlyCollection<VerificationError> newErrors,
        IReadOnlyValueListDictionary<string, VerificationError> persistentErrors,
        IReadOnlyValueListDictionary<string, VerificationError> resolvedErrors)
    {
        NewErrors = newErrors ?? throw new ArgumentNullException(nameof(newErrors));
        PersistentErrors = persistentErrors ?? throw new ArgumentNullException(nameof(persistentErrors));
        ResolvedErrors = resolvedErrors ?? throw new ArgumentNullException(nameof(resolvedErrors));
    }
}
