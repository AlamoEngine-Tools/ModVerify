using System;
using System.Threading;
using AET.ModVerify.Verifiers;
using AnakinRaW.CommonUtilities.SimplePipeline.Steps;
using Microsoft.Extensions.Logging;

namespace AET.ModVerify.Pipeline;

public sealed class GameVerifierPipelineStep(GameVerifier verifier, IServiceProvider serviceProvider) : PipelineStep(serviceProvider)
{
    private readonly GameVerifier _gameVerifier = verifier ?? throw new ArgumentNullException(nameof(verifier));

    protected override void RunCore(CancellationToken token)
    {
        Logger?.LogDebug($"Running verifier '{_gameVerifier.FriendlyName}'...");
        try
        {
            _gameVerifier.Verify(token);
        }
        finally
        {
            Logger?.LogDebug($"Finished verifier '{_gameVerifier.FriendlyName}'");
        }
    }
}