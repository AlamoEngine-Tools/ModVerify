using System;
using AET.ModVerify.Reporting;

namespace AET.ModVerify.Verifiers;

/// <summary>Provides data for the event raised when a verifier reports a verification error.</summary>
/// <param name="error">The verification error that was reported.</param>
/// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/>.</exception>
public sealed class VerificationErrorEventArgs(VerificationError error) : EventArgs
{
    /// <summary>Gets the verification error that was reported.</summary>
    public VerificationError Error { get; } = error ?? throw new ArgumentNullException(nameof(error));
}