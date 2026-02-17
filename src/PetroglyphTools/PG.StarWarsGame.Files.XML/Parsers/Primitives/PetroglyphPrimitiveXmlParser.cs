using System;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphPrimitiveXmlParser<T> : PetroglyphXmlElementParser<T> where T : notnull
{
    private protected abstract T DefaultValue { get; }

    private protected PetroglyphPrimitiveXmlParser() : base(PrimitiveXmlErrorReporter.Instance)
    {
    }

    public sealed override T Parse(XElement element)
    {
        var tagName = element.Name.LocalName;
        if (string.IsNullOrEmpty(tagName))
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.EmptyNodeName,
                Message = "A tag name cannot be null or empty.",
            });
            return DefaultValue;
        }

        if (tagName.Length > XmlFileConstants.MaxTagNameLength)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.TooLongData,
                Message = $"A tag name can be only {XmlFileConstants.MaxTagNameLength} chars long.",
            });
            return DefaultValue;
        }

        var value = element.Value.AsSpan().Trim();
        return value.Length == 0 ? DefaultValue : ParseCore(value, element);
    }

    protected internal abstract T ParseCore(ReadOnlySpan<char> trimmedValue, XElement element);
}