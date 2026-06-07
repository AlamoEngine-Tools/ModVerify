using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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

    public VerificationErrors Categorize(IEnumerable<VerificationError> errors)
    {
        if (errors is null)
            throw new ArgumentNullException(nameof(errors));

        var newErrors = new List<VerificationError>();
        var existingErrors = new Dictionary<string, List<VerificationError>>(StringComparer.Ordinal);
        foreach (var entry in _baselines)
            existingErrors[entry.Identifier] = [];

        foreach (var error in errors)
        {
            if (TryGetMatchingBaseline(error, out var identifier))
                existingErrors[identifier].Add(error);
            else
                newErrors.Add(error);
        }

        var resolvedErrors = new Dictionary<string, IReadOnlyList<VerificationError>>(StringComparer.Ordinal);
        foreach (var entry in _baselines)
        {
            var seen = new HashSet<VerificationError>(existingErrors[entry.Identifier]);
            var solved = new List<VerificationError>();
            foreach (var baselineError in entry.Baseline)
            {
                if (!seen.Contains(baselineError))
                    solved.Add(baselineError);
            }
            resolvedErrors[entry.Identifier] = solved;
        }

        var readOnlyExisting = new Dictionary<string, IReadOnlyList<VerificationError>>(StringComparer.Ordinal);
        foreach (var kvp in existingErrors)
            readOnlyExisting[kvp.Key] = kvp.Value;

        return new VerificationErrors(newErrors, readOnlyExisting, resolvedErrors);
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