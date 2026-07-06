namespace AET.ModVerify.Reporting;

/// <summary>
/// Defines the possible completion statuses of a verification process.
/// </summary>
public enum VerificationCompletionStatus
{
    /// <summary>
    /// Indicates that the verification is completed.
    /// </summary>
    Completed,
    /// <summary>
    /// Indicates that the verification process has completed with failures and will not continue.
    /// </summary>
    CompletedFailFast,
    /// <summary>
    /// Indicates that the verification process was cancelled.
    /// </summary>
    Cancelled,
    /// <summary>
    /// Indicates that the verification process failed.
    /// </summary>
    Failed
}