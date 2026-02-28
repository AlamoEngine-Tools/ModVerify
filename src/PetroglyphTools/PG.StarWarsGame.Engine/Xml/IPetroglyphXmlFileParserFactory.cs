using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.Xml;

public interface IPetroglyphXmlFileParserFactory
{ 
    NamedXmlObjectParser<T> CreateNamedXmlObjectParser<T>(GameEngineType engine, IXmlParserErrorReporter? errorReporter) 
        where T : NamedXmlObject;
}