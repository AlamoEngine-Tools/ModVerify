using System;
using System.Collections.Generic;
using AET.ModVerify;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers;
using PG.StarWarsGame.Engine;

namespace ModVerify.Test.Framework.Providers;

/// <summary>Provides a single verifier instance produced by the given factory.</summary>
internal sealed class SingleVerifierProvider<TVerifier>(
    Func<IStarWarsGameEngine, GameVerifySettings, IServiceProvider, TVerifier> factory)
    : IGameVerifiersProvider
    where TVerifier : GameVerifier
{
    private readonly Func<IStarWarsGameEngine, GameVerifySettings, IServiceProvider, TVerifier> _factory = factory 
        ?? throw new ArgumentNullException(nameof(factory));

    public IEnumerable<GameVerifier> GetVerifiers(
        IStarWarsGameEngine gameEngine, GameVerifySettings settings, IServiceProvider serviceProvider)
    {
        yield return _factory(gameEngine, settings, serviceProvider);
    }
}
