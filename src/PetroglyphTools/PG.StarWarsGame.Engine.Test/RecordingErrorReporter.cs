using System.Collections.Generic;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.Test;

internal sealed class RecordingErrorReporter : IGameEngineErrorReporter
{
    public List<EngineAssert> Asserts { get; } = [];
    
    public List<InitializationError> InitializationErrors { get; } = [];
    
    public List<XmlError> XmlErrors { get; } = [];

    public void Assert(EngineAssert assert)
    {
        Asserts.Add(assert);
    }

    public void Report(InitializationError error)
    {
        InitializationErrors.Add(error);
    }

    public void Report(XmlError error)
    {
        XmlErrors.Add(error);
    }
}
