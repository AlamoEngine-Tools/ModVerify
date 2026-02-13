using System;
using AET.ModVerify.Verifiers;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO;

namespace AET.ModVerify.Reporting.Engine;

internal sealed class InitializationErrorReporter(IGameRepository gameRepository, IServiceProvider serviceProvider) 
    : EngineErrorReporterBase<InitializationError>(gameRepository, serviceProvider)
{
    public override string FriendlyName => "Initialization Errors";

    protected override ErrorData CreateError(InitializationError error)
    {
        return new ErrorData(
            VerifierErrorCodes.InitializationError, 
            error.Message, 
            error.GameManager,
            VerificationSeverity.Critical);
    }
}