using System.Collections.Generic;

namespace AET.ModVerify.Verifiers;

public interface IGameVerifierInfo
{
    IGameVerifierInfo? Parent { get; }

    IReadOnlyList<IGameVerifierInfo> VerifierChain { get; }

    string Name { get; }

    string FriendlyName { get; }
}