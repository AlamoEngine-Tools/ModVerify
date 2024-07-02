using System;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlPrimitiveElementParser<T> : PetroglyphXmlParser<T>, IPetroglyphXmlElementParser<T>
{
    private protected PetroglyphXmlPrimitiveElementParser(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}