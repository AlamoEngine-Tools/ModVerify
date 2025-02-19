using System;
using System.Collections.Generic;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers;
using PG.StarWarsGame.Engine.Database;

namespace AET.ModVerify;

internal class VerificationProvider(IServiceProvider serviceProvider) : IVerificationProvider
{
    public IEnumerable<GameVerifierBase> GetAllDefaultVerifiers(IGameDatabase database, GameVerifySettings settings)
    {
        //yield return new ReferencedModelsVerifier(database, settings, serviceProvider);
        //yield return new DuplicateNameFinder(database, settings, serviceProvider);
        //yield return new AudioFilesVerifier(database, settings, serviceProvider);
        yield return new ReferencedTexturesVerifier(database, settings, serviceProvider);
    }
}