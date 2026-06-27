using System;
using System.Collections.Generic;
using AET.ModVerify.Reporting.Baseline;
using AET.ModVerify.Reporting.Suppressions;
using AET.ModVerify.Verifiers;

namespace AET.ModVerify.Reporting;

/// <summary>
/// Represents the result of a verification process.
/// </summary>
public sealed record VerificationResult
{
    /// <summary>
    /// Gets or sets the completion status of the verification process.
    /// </summary>
    public required VerificationCompletionStatus Status { get; init; }

    /// <summary>
    /// Gets or sets the collection of verification errors that were found during the verification process.
    /// </summary>
    public required CategorizedVerificationErrors Errors
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the collection of baselines that were used during the verification process.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public required BaselineCollection UsedBaselines
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the collection of suppressions that were used during the verification process.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public required SuppressionList UsedSuppressions
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the collection of game verifiers that were executed during the verification process.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public required IReadOnlyCollection<IGameVerifierInfo> Verifiers
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the target of the verification process.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public required VerificationTarget Target
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets the duration of the verification process.
    /// </summary>
    public required TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets or sets the exception that was thrown during the verification process, if any.
    /// </summary>
    public Exception? Exception { get; init; }
}
