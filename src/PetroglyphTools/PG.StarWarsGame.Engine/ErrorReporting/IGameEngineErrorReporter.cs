using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.ErrorReporting;

public interface IGameEngineErrorReporter : IXmlParserErrorReporter
{
    void Report(InitializationError error);

    void Assert(EngineAssert assert);
}