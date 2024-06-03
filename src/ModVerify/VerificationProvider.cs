﻿using System;
using System.Collections.Generic;
using AET.ModVerify.Steps;
using PG.StarWarsGame.Engine.Database;

namespace AET.ModVerify;

internal class VerificationProvider(IServiceProvider serviceProvider) : IVerificationProvider
{
    public IEnumerable<GameVerificationStep> GetAllDefaultVerifiers(IGameDatabase database, VerificationSettings settings)
    {
        yield return new VerifyReferencedModelsStep(database, settings, serviceProvider);
    }
}