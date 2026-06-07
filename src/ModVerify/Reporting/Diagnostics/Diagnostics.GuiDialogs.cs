using System.Collections.Generic;
using AET.ModVerify.Verifiers;

namespace AET.ModVerify.Reporting.Diagnostics;

/// <summary>Provides factories for findings produced while verifying GUI dialog textures.</summary>
public static class GuiDialogs
{
    private static readonly ErrorDescriptor _mtdFileNotFound = new(
        VerifierErrorCodes.FileNotFound, "GuiMtdFileNotFound", VerificationSeverity.Critical, "GuiDialogs");

    private static readonly ErrorDescriptor _textureNameTooLong = new(
        VerifierErrorCodes.FilePathTooLong, "GuiTextureNameTooLong", VerificationSeverity.Error, "GuiDialogs");

    private static readonly ErrorDescriptor _guiTextureNotFound = new(
        VerifierErrorCodes.FileNotFound, "GuiTextureNotFound", VerificationSeverity.Error, "GuiDialogs");

    /// <summary>Creates an error for the GUI dialogs mega-texture directory file that could not be found.</summary>
    public static VerificationError MtdFileNotFound(IGameVerifierInfo verifier, string mtdFileName, IEnumerable<string> context)
        => _mtdFileNotFound.Create(verifier, $"MtdFile '{mtdFileName}.mtd' could not be found.", mtdFileName, context);

    /// <summary>Creates an error for a GUI texture name that exceeds the mega-texture maximum length.</summary>
    public static VerificationError TextureNameTooLong(IGameVerifierInfo verifier, string textureName, int maxLength, IEnumerable<string> context)
        => _textureNameTooLong.Create(verifier, $"The filename is too long. Max length is {maxLength} characters.", textureName, context);

    /// <summary>Creates an error for a GUI dialog texture that could not be found, using a caller-assembled message.</summary>
    public static VerificationError GuiTextureNotFound(IGameVerifierInfo verifier, string textureName, string message, IEnumerable<string> context)
        => _guiTextureNotFound.Create(verifier, message, textureName, context);
}
