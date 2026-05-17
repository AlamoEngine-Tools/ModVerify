using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers;
using ModVerify.Test.Framework.Providers;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Testing;

namespace ModVerify.Test.Framework;

public abstract class VerifierTestBase<TVerifier> : ModVerifyTestBase
    where TVerifier : GameVerifier
{
    protected async Task<IReadOnlyCollection<VerificationError>> RunAsync(
        VirtualGameRepo repo,
        Func<IStarWarsGameEngine, GameVerifySettings, IServiceProvider, TVerifier> factory,
        VerifierServiceSettings? settings = null)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        var provider = new SingleVerifierProvider<TVerifier>(factory);
        var result = await RunPipelineAsync(repo, verifiers: provider, settings: settings).ConfigureAwait(false);
        return result.NewErrors.Concat(result.ExistingErrors.Values.SelectMany(v => v)).ToList();
    }
}
