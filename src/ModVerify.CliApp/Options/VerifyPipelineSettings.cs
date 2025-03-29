using AET.ModVerify.Settings;

namespace AET.ModVerifyTool.Options;

internal sealed class VerifyPipelineSettings
{
    public required GameVerifySettings GameVerifySettings { get; init; }

    public required IGameVerifierFactory VerifierFactory { get; init; }

    public bool FailFast { get; init; }

    public int ParallelVerifiers { get; init; } = 4;
}