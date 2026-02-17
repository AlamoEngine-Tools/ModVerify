using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal interface IPetroglyphXmlFileParserFactory
{ 
    NamedXmlObjectParser<T> CreateNamedXmlObjectParser<T>(IXmlParserErrorReporter? errorReporter) where T : NamedXmlObject;
}