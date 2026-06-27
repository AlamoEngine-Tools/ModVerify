using System.Collections.Generic;
using AET.ModVerify.Verifiers;

namespace AET.ModVerify.Reporting.Diagnostics;

/// <summary>Provides factories for findings produced while verifying textures.</summary>
public static class TextureErrors
{
    private static readonly ErrorDescriptor _pathTooLong = new(
        VerifierErrorCodes.FilePathTooLong, "TexturePathTooLong", VerificationSeverity.Error, "Textures");

    private static readonly ErrorDescriptor _notFound = new(
        VerifierErrorCodes.FileNotFound, "TextureNotFound", VerificationSeverity.Error, "Textures");

    /// <summary>Creates an error for a texture that could not be found because the resolved path is too long.</summary>
    public static VerificationError PathTooLong(IGameVerifierInfo verifier, string texturePath, IEnumerable<string> context)
        => _pathTooLong.Create(verifier, $"Could not find texture '{texturePath}' because the engine resolved a path that is too long.", texturePath, context);

    /// <summary>Creates an error for a texture that could not be found.</summary>
    public static VerificationError NotFound(IGameVerifierInfo verifier, string texturePath, IEnumerable<string> context)
        => _notFound.Create(verifier, $"Could not find texture '{texturePath}'.", texturePath, context);
}
