using System.Collections.Generic;

namespace AET.ModVerify.Verifiers.Utilities;

internal static class GameVerifierInfoExtensions
{
    public static IReadOnlyList<IGameVerifierInfo> GetVerifierChain(this IGameVerifierInfo verifier)
    {
        if (verifier.Parent is null)
            return [verifier];

        var parentChain = verifier.Parent.VerifierChain;
        var result = new List<IGameVerifierInfo>(parentChain.Count + 1);
        result.AddRange(parentChain);
        result.Add(verifier);
        return result;
    }
}
