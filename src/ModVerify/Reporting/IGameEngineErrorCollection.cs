using System.Collections.Generic;
using PG.StarWarsGame.Engine.ErrorReporting;

namespace AET.ModVerify.Reporting;

public interface IGameEngineErrorCollection
{
    IEnumerable<XmlError> XmlErrors { get; }
    IEnumerable<InitializationError> InitializationErrors { get; }
    IEnumerable<EngineAssert> Asserts { get; }
}