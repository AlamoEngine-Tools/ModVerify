using System.Collections.Generic;

namespace AET.ModVerify.Reporting;

internal class VerificationErrorContextEqualityComparer : IEqualityComparer<HashSet<string>>
{
    readonly IEqualityComparer<HashSet<string>> _setComparer = HashSet<string>.CreateSetComparer();

    public static VerificationErrorContextEqualityComparer Instance { get; } = new();

    private VerificationErrorContextEqualityComparer()
    {
    }

    public bool Equals(HashSet<string> x, HashSet<string> y)
    {
        return _setComparer.Equals(x, y);
    }

    public int GetHashCode(HashSet<string> obj)
    {
        return _setComparer.GetHashCode(obj);
    }
}