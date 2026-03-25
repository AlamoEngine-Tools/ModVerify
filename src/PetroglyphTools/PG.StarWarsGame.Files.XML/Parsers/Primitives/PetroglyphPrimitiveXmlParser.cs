using PG.StarWarsGame.Files.XML.ErrorHandling;
using System;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphPrimitiveXmlParser<T> : PetroglyphXmlParserBase where T : notnull
{
    private protected abstract T DefaultValue { get; }

    internal abstract int EngineDataTypeId { get; }

    private protected PetroglyphPrimitiveXmlParser() : base(PrimitiveXmlErrorReporter.Instance)
    {
    }

    public T Parse(XElement element)
    {
        if (!IsTagValid(element))
            return DefaultValue;
        var value = element.PGValue.AsSpan().Trim();
        return value.Length == 0 ? DefaultValue : ParseCore(value, element);
    }

    protected internal abstract T ParseCore(ReadOnlySpan<char> trimmedValue, XElement element);
}