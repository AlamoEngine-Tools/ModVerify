namespace PG.StarWarsGame.Files.XML.ErrorHandling;

public interface IXmlParserErrorReporter
{
    void Report(string parser, XmlParseErrorEventArgs error);
}