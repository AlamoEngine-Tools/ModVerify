using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal interface IPetroglyphXmlFileParserFactory
{ 
    NamedXmlObjectParser<T> CreateNamedXmlObjectParser<T>(IXmlParserErrorReporter? errorReporter) where T : NamedXmlObject;
}