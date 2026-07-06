namespace AET.ModVerify.Progress;

/// <summary>
/// Contains information about the progress of a verification process.
/// </summary>
public readonly struct VerifyProgressInfo
{
    /// <summary>
    /// Gets or sets a value indicating whether the progress contains more detailed or additional information.
    /// </summary>
    public bool IsDetailed { get; init; }

    /// <summary>
    /// Gets the total number of verifiers that are being executed in the verification process.
    /// </summary>
    public int TotalVerifiers { get; internal init; }
}