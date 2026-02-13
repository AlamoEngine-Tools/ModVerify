using System;
using System.Collections.Generic;
using AET.ModVerify.Verifiers;

namespace AET.ModVerify.Reporting;

public sealed record VerificationResult
{
    public required VerificationCompletionStatus Status { get; init; }

    public required IReadOnlyCollection<VerificationError> Errors
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    }

    public required VerificationBaseline UsedBaseline
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    }

    public required SuppressionList UsedSuppressions
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    }

    public required IReadOnlyCollection<IGameVerifierInfo> Verifiers 
    { 
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    }

    public required VerificationTarget Target
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    }

    public required TimeSpan Duration { get; init; }
}