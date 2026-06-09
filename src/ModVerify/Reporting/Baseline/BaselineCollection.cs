using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AnakinRaW.CommonUtilities.Collections;

namespace AET.ModVerify.Reporting.Baseline;

public sealed class BaselineCollection : IReadOnlyCollection<IdentifiedBaseline>
{
    public static readonly BaselineCollection Empty = new([]);

    private readonly IReadOnlyList<IdentifiedBaseline> _baselines;

    public int Count => _baselines.Count;

    public bool IsEmpty => _baselines.Count == 0;

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
    
    public bool Contains(VerificationError error)
    {
        foreach (var entry in _baselines)
        {
            if (entry.Baseline.Contains(error))
                return true;
        }
        return false;
    }

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

    public IEnumerable<VerificationError> Apply(IEnumerable<VerificationError> errors)
    {
        if (errors is null)
            throw new ArgumentNullException(nameof(errors));
        if (_baselines.Count == 0)
            return errors;
        return errors.Where(e => !Contains(e));
    }

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

    public IEnumerator<IdentifiedBaseline> GetEnumerator()
    {
        return _baselines.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}