using System.Collections.Generic;
using AET.ModVerify.Verifiers;

namespace AET.ModVerify.Reporting.Diagnostics;

/// <summary>Provides factories for findings that are not specific to a single verifier family.</summary>
public static class CommonErrors
{
    private static readonly ErrorDescriptor _duplicate = new(
        VerifierErrorCodes.Duplicate, "DuplicateEntry", VerificationSeverity.Error, "Common");

    /// <summary>Creates an error for a duplicate entry, using a caller-supplied message.</summary>
    public static VerificationError Duplicate(IGameVerifierInfo verifier, string asset, string message, IEnumerable<string> context)
        => _duplicate.Create(verifier, message, asset, context);
}
