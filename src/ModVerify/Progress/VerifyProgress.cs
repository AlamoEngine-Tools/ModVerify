using AnakinRaW.CommonUtilities.SimplePipeline.Progress;

namespace AET.ModVerify.Progress;

/// <summary>
/// Provides progress types for the verification process.
/// </summary>
public static class VerifyProgress
{
    /// <summary>
    /// Gets the progress type for the verification process.
    /// </summary>
    public static readonly ProgressType ProgressType = new()
    {
        Id = "Verify",
        DisplayName = "Verify"
    };
}