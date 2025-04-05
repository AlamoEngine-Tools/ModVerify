namespace AET.ModVerify.Pipeline.Progress;

public struct VerifyProgressInfo
{
    public bool IsDetailed { get; init; }

    public int TotalVerifiers { get; internal set; }
}