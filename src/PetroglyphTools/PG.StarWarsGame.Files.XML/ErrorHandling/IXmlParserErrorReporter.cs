namespace PG.StarWarsGame.Files.XML.ErrorHandling;

public interface IXmlParserErrorReporter
{
    void Report(XmlError error);
}