using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers;
using PG.StarWarsGame.Engine.Database;
using System;
using System.Collections.Generic;

namespace AET.ModVerifyTool;

public interface IGameVerifierFactory
{
    IEnumerable<GameVerifier> GetVerifiers(
        IGameDatabase database, 
        GameVerifySettings settings, 
        IServiceProvider serviceProvider);
}