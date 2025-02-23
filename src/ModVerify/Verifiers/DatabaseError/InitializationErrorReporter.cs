﻿using System;
using AET.ModVerify.Reporting;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO;

namespace AET.ModVerify.Verifiers;

internal sealed class InitializationErrorReporter(IGameRepository gameRepository, IServiceProvider serviceProvider) : InitializationErrorReporterBase<InitializationError>(gameRepository, serviceProvider)
{
    public override string Name => "InitializationErrors";

    protected override void CreateError(InitializationError error, out ErrorData errorData)
    {
        errorData = new ErrorData("INIT00", error.Message, [error.GameManager], VerificationSeverity.Critical);
    }
}