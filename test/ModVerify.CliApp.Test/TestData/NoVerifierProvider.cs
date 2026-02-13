using System;
using System.Collections.Generic;
using AET.ModVerify;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers;
using PG.StarWarsGame.Engine;

namespace ModVerify.CliApp.Test.TestData;

internal sealed class NoVerifierProvider : IGameVerifiersProvider
{
    public IEnumerable<GameVerifier> GetVerifiers(IStarWarsGameEngine database, GameVerifySettings settings, IServiceProvider serviceProvider)
    {
        yield break;
    }
}