using AET.ModVerify.Verifiers;

namespace AET.ModVerify.Reporting;

internal sealed class RestoredVerifierInfo : IGameVerifierInfo
{
    public IGameVerifierInfo? Parent { get; init; }
    public required string Name { get; init; }
    public string FriendlyName => Name;
}