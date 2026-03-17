using AET.ModVerify.Verifiers;
using AET.ModVerify.Verifiers.Utilities;
using System.Collections.Generic;

namespace AET.ModVerify.Reporting;

internal sealed class RestoredVerifierInfo : IGameVerifierInfo
{
    public IGameVerifierInfo? Parent { get; init; }

    public IReadOnlyList<IGameVerifierInfo> VerifierChain => field ??= this.GetVerifierChain();

    public required string Name { get; init; }
    public string FriendlyName => Name;
}