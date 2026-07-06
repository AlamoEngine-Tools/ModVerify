using System.Collections.Generic;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace AET.ModVerify.Reporting.Engine;

/// <summary>Provides the errors and assertions collected from the game engine during verification.</summary>
public interface IGameEngineErrorCollection
{
    /// <summary>Gets the XML parser errors reported by the engine.</summary>
    IEnumerable<XmlError> XmlErrors { get; }

    /// <summary>Gets the initialization errors reported by the engine.</summary>
    IEnumerable<InitializationError> InitializationErrors { get; }

    /// <summary>Gets the assertions raised by the engine.</summary>
    IEnumerable<EngineAssert> Asserts { get; }
}