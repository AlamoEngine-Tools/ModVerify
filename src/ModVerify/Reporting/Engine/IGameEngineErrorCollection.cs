using System.Collections.Generic;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace AET.ModVerify.Reporting.Engine;

public interface IGameEngineErrorCollection
{
    IEnumerable<XmlError> XmlErrors { get; }
    IEnumerable<InitializationError> InitializationErrors { get; }
    IEnumerable<EngineAssert> Asserts { get; }
}