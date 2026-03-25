using System;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.ErrorReporting;

internal sealed class GameEngineErrorReporterWrapper(IGameEngineErrorReporter? errorReporter)
    : XmlErrorReporter, IGameEngineErrorReporter
{
    internal event EventHandler<InitializationError>? InitializationError;

    public override void Report(XmlError error)
    {
        errorReporter?.Report(error);
    }

    public void Report(InitializationError error)
    {
        InitializationError?.Invoke(this, error);
        errorReporter?.Report(error);
    }

    public void Assert(EngineAssert assert)
    {
        errorReporter?.Assert(assert);
    }
}