namespace AET.ModVerify;

public record GameVerifySettings
{
    public int ParallelWorkers { get; init; } = 4;

    public static readonly GameVerifySettings Default = new()
    {
        ThrowBehavior = VerifyThrowBehavior.None
    };

    public VerifyThrowBehavior ThrowBehavior { get; init; }

    public VerifyLocalizationOption VerifyLocalization { get; init; }
}

public enum VerifyLocalizationOption
{
    English,
    CurrentSystem,
    AllInstalled,
    All
}