using System;
using System.Collections.Generic;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers;
using PG.StarWarsGame.Engine.Database;

namespace AET.ModVerify.Pipeline;

public interface IGameVerifiersProvider
{
    IEnumerable<GameVerifier> GetVerifiers(
        IGameDatabase database, 
        GameVerifySettings settings, 
        IServiceProvider serviceProvider);
}