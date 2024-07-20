using System.Collections.Generic;

namespace AET.ModVerify.Reporting;

internal class AssetsEqualityComparer : IEqualityComparer<HashSet<string>>
{
    readonly IEqualityComparer<HashSet<string>> _setComparer = HashSet<string>.CreateSetComparer();

    public static AssetsEqualityComparer Instance { get; } = new();

    private AssetsEqualityComparer()
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