using System;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public sealed class PetroglyphXmlStringParser : PetroglyphXmlPrimitiveElementParser<string>
{
    internal PetroglyphXmlStringParser(IServiceProvider serviceProvider, IPrimitiveXmlParserErrorListener listener) : base(serviceProvider, listener)
    {
    }

    public override string Parse(XElement element)
    {
        return element.Value.Trim();
    }
}