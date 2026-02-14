namespace AET.ModVerify.Progress;

public struct VerifyProgressInfo
{
    public bool IsDetailed { get; init; }

    public int TotalVerifiers { get; internal init; }
}