using System;

namespace AET.ModVerify.Reporting.Baseline;

public sealed record IdentifiedBaseline
{
    public string Identifier { get; }

    public VerificationBaseline Baseline { get; }

    public BaselineSource Source { get; }

    public IdentifiedBaseline(string identifier, VerificationBaseline baseline, BaselineSource source)
    {
        if (string.IsNullOrEmpty(identifier))
            throw new ArgumentException("Identifier must be non-empty.", nameof(identifier));
        Identifier = identifier;
        Baseline = baseline ?? throw new ArgumentNullException(nameof(baseline));
        Source = source;
    }
}
