namespace PG.StarWarsGame.Files.XML.ErrorHandling;

public interface IXmlParserErrorProvider
{
    event XmlErrorEventHandler XmlParseError;
}