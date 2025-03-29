using AET.ModVerify.Pipeline;

namespace AET.ModVerify.Settings;

public sealed class VerifyPipelineSettings
{
    public required GameVerifySettings GameVerifySettings { get; init; }

    public required IGameVerifiersProvider VerifiersProvider { get; init; }

    public bool FailFast { get; init; }

    public int ParallelVerifiers { get; init; } = 4;
}