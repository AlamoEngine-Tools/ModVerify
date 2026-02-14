using System;
using System.Collections.Generic;
using AET.ModVerify.Verifiers;

namespace AET.ModVerify.Reporting;

internal sealed class VerifierChainEqualityComparer : IEqualityComparer<IReadOnlyList<IGameVerifierInfo>>
{
    public static readonly VerifierChainEqualityComparer Instance = new();

    private VerifierChainEqualityComparer()
    {
    }

    public bool Equals(IReadOnlyList<IGameVerifierInfo>? x, IReadOnlyList<IGameVerifierInfo>? y)
    {
        if (ReferenceEquals(x, y)) 
            return true;
        if (x is null || y is null) 
            return false;
        if (x.Count != y.Count) 
            return false;
        for (var i = 0; i < x.Count; i++)
        {
            if (!NameBasedEqualityComparer.Instance.Equals(x[i], y[i]))
                return false;
        }
        return true;
    }

    public int GetHashCode(IReadOnlyList<IGameVerifierInfo>? obj)
    {
        if (obj == null) 
            return 0;
        var hashCode = new HashCode();
        foreach (var verifier in obj) 
            hashCode.Add(verifier.Name);
        return hashCode.ToHashCode();
    }
}
