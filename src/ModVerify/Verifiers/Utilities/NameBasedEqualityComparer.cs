using System.Collections.Generic;

namespace AET.ModVerify.Verifiers.Utilities;

internal sealed class NameBasedEqualityComparer : IEqualityComparer<GameVerifier>, IEqualityComparer<IGameVerifierInfo>
{
    public static readonly NameBasedEqualityComparer Instance = new();

    public bool Equals(GameVerifier? x, GameVerifier? y)
    {
        return Equals(x as IGameVerifierInfo, y);
    }

    public int GetHashCode(GameVerifier? obj)
    {
        return GetHashCode(obj as IGameVerifierInfo);
    }

    public bool Equals(IGameVerifierInfo? x, IGameVerifierInfo? y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (x is null)
            return false;
        if (y is null)
            return false;
        return x.Name == y.Name;
    }

    public int GetHashCode(IGameVerifierInfo? obj)
    {
        return obj?.Name.GetHashCode() ?? 0;
    }
}