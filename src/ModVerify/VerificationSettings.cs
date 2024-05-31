namespace AET.ModVerify;

public record VerificationSettings
{
    public static readonly VerificationSettings Default = new()
    {
        ThrowBehavior = VerifyThrowBehavior.None
    };

    public VerifyThrowBehavior ThrowBehavior { get; init; }
}