using System;
using System.Collections.Generic;
using AET.ModVerify;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers;
using PG.StarWarsGame.Engine;

namespace ModVerify.Test.Framework.Providers;

internal sealed class NoVerifiersProvider : IGameVerifiersProvider
{
    public IEnumerable<GameVerifier> GetVerifiers(
        IStarWarsGameEngine gameEngine, GameVerifySettings settings, IServiceProvider sp)
    {
        return [];
    }
}
