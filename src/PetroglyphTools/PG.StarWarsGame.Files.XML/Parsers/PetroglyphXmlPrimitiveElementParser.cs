using System;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlPrimitiveElementParser<T> : PetroglyphXmlParser<T>, IPetroglyphXmlElementParser<T>
{
    private protected PetroglyphXmlPrimitiveElementParser(IServiceProvider serviceProvider, IPrimitiveXmlParserErrorListener listener) :
        base(serviceProvider, listener)
    {
    }
}