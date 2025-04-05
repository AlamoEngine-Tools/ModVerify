using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlElementParser<T>(IXmlParserErrorReporter? errorReporter = null)
    : PetroglyphXmlParserBase(errorReporter), IPetroglyphXmlElementParser<T> where T : notnull
{
    public abstract T Parse(XElement element);
}