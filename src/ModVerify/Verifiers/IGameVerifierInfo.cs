namespace AET.ModVerify.Verifiers;

public interface IGameVerifierInfo
{
    IGameVerifierInfo? Parent { get; }

    string Name { get; }

    string FriendlyName { get; }
}