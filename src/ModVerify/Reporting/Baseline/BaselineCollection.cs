using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AnakinRaW.CommonUtilities.Collections;

namespace AET.ModVerify.Reporting.Baseline;

/// <summary>Represents a set of identified verification baselines, each distinguished by a unique identifier.</summary>
public sealed class BaselineCollection : IReadOnlyCollection<IdentifiedBaseline>
{
    /// <summary>Gets an empty <see cref="BaselineCollection"/> that contains no baselines.</summary>
    public static readonly BaselineCollection Empty = new([]);

    private readonly IReadOnlyList<IdentifiedBaseline> _baselines;

    /// <inheritdoc />
    public int Count => _baselines.Count;

    /// <summary>Gets a value that indicates whether the collection contains no baselines.</summary>
    /// <value><see langword="true"/> if the collection contains no baselines; otherwise, <see langword="false"/>.</value>
    public bool IsEmpty => _baselines.Count == 0;

    /// <summary>Initializes a new instance of the <see cref="BaselineCollection"/> class with the specified baselines.</summary>
    /// <param name="baselines">The identified baselines to include in the collection.</param>
    /// <exception cref="ArgumentNullException"><paramref name="baselines"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">An entry in <paramref name="baselines"/> is <see langword="null"/>, or two entries share the same identifier.</exception>
    public BaselineCollection(IEnumerable<IdentifiedBaseline> baselines)
    {
        if (baselines is null)
            throw new ArgumentNullException(nameof(baselines));

        var list = new List<IdentifiedBaseline>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var b in baselines)
        {
            if (b is null)
                throw new ArgumentException("Baseline entries must not be null.", nameof(baselines));
            if (!seen.Add(b.Identifier))
                throw new ArgumentException($"Baseline identifier '{b.Identifier}' is not unique within the collection.", nameof(baselines));
            list.Add(b);
        }
        _baselines = list;
    }
    
    /// <summary>Determines whether any baseline in the collection contains the specified error.</summary>
    /// <param name="error">The verification error to locate.</param>
    /// <returns><see langword="true"/> if a baseline in the collection contains <paramref name="error"/>; otherwise, <see langword="false"/>.</returns>
    public bool Contains(VerificationError error)
    {
        foreach (var entry in _baselines)
        {
            if (entry.Baseline.Contains(error))
                return true;
        }
        return false;
    }

    /// <summary>Gets the identifier of the first baseline in the collection that contains the specified error.</summary>
    /// <param name="error">The verification error to locate.</param>
    /// <param name="identifier">When this method returns, contains the identifier of the matching baseline if a match was found, or <see langword="null"/> if no baseline contains the error. This parameter is treated as uninitialized.</param>
    /// <returns><see langword="true"/> if a baseline containing <paramref name="error"/> was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetMatchingBaseline(VerificationError error, [NotNullWhen(true)] out string? identifier)
    {
        foreach (var entry in _baselines)
        {
            if (entry.Baseline.Contains(error))
            {
                identifier = entry.Identifier;
                return true;
            }
        }
        identifier = null;
        return false;
    }

    /// <summary>Filters out the errors that are contained in any baseline of the collection.</summary>
    /// <param name="errors">The errors to filter.</param>
    /// <returns>The errors that are not contained in any baseline of the collection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="errors"/> is <see langword="null"/>.</exception>
    public IEnumerable<VerificationError> Apply(IEnumerable<VerificationError> errors)
    {
        if (errors is null)
            throw new ArgumentNullException(nameof(errors));
        if (_baselines.Count == 0)
            return errors;
        return errors.Where(e => !Contains(e));
    }

    /// <summary>Categorizes the specified errors into new, persistent, and resolved errors relative to the baselines in the collection.</summary>
    /// <param name="errors">The errors found during verification.</param>
    /// <returns>
    /// The errors grouped into those not present in any baseline, those matching a baseline, and those present in a
    /// baseline but not found again during verification.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="errors"/> is <see langword="null"/>.</exception>
    public CategorizedVerificationErrors Categorize(IEnumerable<VerificationError> errors)
    {
        if (errors is null)
            throw new ArgumentNullException(nameof(errors));

        var newErrors = new List<VerificationError>();
        var existing = new ValueListDictionary<string, VerificationError>();

        foreach (var error in errors)
        {
            if (TryGetMatchingBaseline(error, out var identifier))
                existing.Add(identifier, error);
            else
                newErrors.Add(error);
        }

        var resolved = new ValueListDictionary<string, VerificationError>();
        foreach (var entry in _baselines)
        {
            existing.TryGetValues(entry.Identifier, out var matched);
            var seen = new HashSet<VerificationError>(matched);
            foreach (var baselineError in entry.Baseline)
            {
                if (!seen.Contains(baselineError))
                    resolved.Add(entry.Identifier, baselineError);
            }
        }

        return new CategorizedVerificationErrors(
            newErrors,
            new ReadOnlyValueListDictionary<string, VerificationError>(existing),
            new ReadOnlyValueListDictionary<string, VerificationError>(resolved));
    }

    /// <inheritdoc />
    public IEnumerator<IdentifiedBaseline> GetEnumerator()
    {
        return _baselines.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}