using System.Collections.Generic;

namespace AET.ModVerify.Verifiers;

/// <summary>Provides identity and lineage information about a game verifier.</summary>
public interface IGameVerifierInfo
{
    /// <summary>Gets the parent verifier that created this verifier, or <see langword="null"/> if this is a root verifier.</summary>
    IGameVerifierInfo? Parent { get; }

    /// <summary>Gets the chain of verifiers from the root down to and including this verifier.</summary>
    IReadOnlyList<IGameVerifierInfo> VerifierChain { get; }

    /// <summary>Gets the unique name of the verifier.</summary>
    string Name { get; }

    /// <summary>Gets the human-readable name of the verifier.</summary>
    string FriendlyName { get; }
}