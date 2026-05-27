using System.Collections.Generic;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.Test;

/// <summary>Test double that captures every call made to an <see cref="IGameEngineErrorReporter"/>.</summary>
internal sealed class RecordingErrorReporter : IGameEngineErrorReporter
{
    public List<EngineAssert> Asserts { get; } = new();
    public List<InitializationError> InitializationErrors { get; } = new();
    public List<XmlError> XmlErrors { get; } = new();

    public void Assert(EngineAssert assert) => Asserts.Add(assert);
    public void Report(InitializationError error) => InitializationErrors.Add(error);
    public void Report(XmlError error) => XmlErrors.Add(error);
}
