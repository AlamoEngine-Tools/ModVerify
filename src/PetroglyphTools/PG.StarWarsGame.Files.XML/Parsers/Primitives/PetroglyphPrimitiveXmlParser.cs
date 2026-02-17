using System;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphPrimitiveXmlParser<T> : PetroglyphXmlParserBase where T : notnull
{
    private protected abstract T DefaultValue { get; }

    private protected PetroglyphPrimitiveXmlParser() : base(PrimitiveXmlErrorReporter.Instance)
    {
    }

    public T Parse(XElement element)
    {
        if (IsTagValid(element))
            return DefaultValue;
        var value = element.Value.AsSpan().Trim();
        return value.Length == 0 ? DefaultValue : ParseCore(value, element);
    }

    protected internal abstract T ParseCore(ReadOnlySpan<char> trimmedValue, XElement element);
}