using System.Collections.Generic;
using AET.ModVerify.Verifiers;

namespace AET.ModVerify.Reporting.Diagnostics;

/// <summary>Provides factories for findings produced while verifying SFX events.</summary>
public static class SfxErrors
{
    private static readonly ErrorDescriptor _nameTooLong = new(
        VerifierErrorCodes.NameTooLong, "SfxEventNameTooLong", VerificationSeverity.Critical, "Sfx");

    private static readonly ErrorDescriptor _missingPreset = new(
        VerifierErrorCodes.MissingXRef, "SfxMissingPreset", VerificationSeverity.Error, "Sfx");

    /// <summary>Creates an error for an SFX event name that exceeds the maximum length.</summary>
    public static VerificationError NameTooLong(IGameVerifierInfo verifier, string name, int maxLength, IEnumerable<string> context)
        => _nameTooLong.Create(verifier, $"The SFXEvent name '{name}' is too long. Maximum length is {maxLength}.", name, context);

    /// <summary>Creates an error for an SFX event that references a missing preset.</summary>
    public static VerificationError MissingPreset(IGameVerifierInfo verifier, string presetName, string eventName, IEnumerable<string> context)
        => _missingPreset.Create(verifier, $"Missing preset '{presetName}' for SFXEvent '{eventName}'.", presetName, context);
}
