using System;
using AET.ModVerify.Reporting;

namespace AET.ModVerify.Verifiers;

public sealed class VerificationErrorEventArgs(VerificationError error) : EventArgs
{
    public VerificationError Error { get; } = error ?? throw new ArgumentNullException(nameof(error));
}