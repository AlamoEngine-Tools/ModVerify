using AnakinRaW.CommonUtilities.SimplePipeline.Progress;

namespace AET.ModVerify.Pipeline.Progress;

public static class VerifyProgress
{
    public static readonly ProgressType ProgressType = new()
    {
        Id = "Verify",
        DisplayName = "Verify"
    };
}