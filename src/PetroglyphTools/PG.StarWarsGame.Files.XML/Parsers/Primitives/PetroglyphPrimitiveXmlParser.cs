using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public abstract class PetroglyphPrimitiveXmlParser<T> : PetroglyphXmlElementParser<T> where T : notnull
{
    private protected PetroglyphPrimitiveXmlParser() : base(PrimitiveXmlErrorReporter.Instance)
    {
    }
}