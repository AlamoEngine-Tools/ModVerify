namespace AET.ModVerify.Settings;

public sealed class VerifierServiceSettings
{
    public required GameVerifySettings GameVerifySettings { get; init; }

    public required IGameVerifiersProvider VerifiersProvider { get; init; }

    public FailFastSetting FailFastSettings { get; init; } = FailFastSetting.NoFailFast;

    public int ParallelVerifiers { get; init; } = 4;
}