namespace AET.ModVerify;

public record ModVerifySettings
{
    public int ParallelWorkers { get; init; } = 4;

    public static readonly ModVerifySettings Default = new()
    {
        ThrowBehavior = VerifyThrowBehavior.None
    };

    public VerifyThrowBehavior ThrowBehavior { get; init; }
}