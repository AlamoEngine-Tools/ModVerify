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
            ErrorReporter?.Report(this, new XmlParseErrorEventArgs(element, XmlParseErrorKind.EmptyNodeName, "A tag name cannot be null or empty."));
            return DefaultValue;
        }
        if (tagName.Length >= 256) 
            ErrorReporter?.Report(this, new XmlParseErrorEventArgs(element, XmlParseErrorKind.TooLongData, "A tag name cannot be null or empty."));

        var value = element.Value.Trim();
        return value.Length == 0 ? DefaultValue : ParseCore(value, element);
    }

    protected internal abstract T ParseCore(string trimmedValue, XElement element);
}