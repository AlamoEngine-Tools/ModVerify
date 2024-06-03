namespace AET.ModVerify;

public record VerificationSettings
{
    public int ParallelWorkers { get; init; } = 4;

    public static readonly VerificationSettings Default = new()
    {
        ThrowBehavior = VerifyThrowBehavior.None
    };

    public VerifyThrowBehavior ThrowBehavior { get; init; }
}