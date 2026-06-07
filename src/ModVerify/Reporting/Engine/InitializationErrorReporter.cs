using System;
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
            Diagnostics.Engine.InitializationError.Id,
            error.Message,
            error.GameManager,
            Diagnostics.Engine.InitializationError.Severity);
    }
}