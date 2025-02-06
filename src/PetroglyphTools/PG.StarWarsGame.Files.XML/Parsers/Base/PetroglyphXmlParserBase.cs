using System.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlParserBase(IXmlParserErrorReporter? errorReporter)
{
    protected readonly IXmlParserErrorReporter? ErrorReporter = errorReporter;

    public sealed override string ToString()
    {
        return GetType().FullName!;
    }

    protected string GetTagName(XElement element)
    {
        return element.Name.LocalName;
    }

    protected string GetNameAttributeValue(XElement element)
    {
        var nameAttribute = element.Attributes()
            .FirstOrDefault(a => a.Name.LocalName == "Name");
        return nameAttribute is null ? string.Empty : nameAttribute.Value;
    }

    protected bool GetNameAttributeValue(XElement element, out string value)
    {
        return GetAttributeValue(element, "Name", out value!, string.Empty);
    }

    protected bool GetAttributeValue(XElement element, string attribute, out string? value, string? defaultValue = null)
    {
        var nameAttribute = element.Attributes()
            .FirstOrDefault(a => a.Name.LocalName == attribute);

        if (nameAttribute is null)
        {
            value = defaultValue;
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.MissingAttribute, $"Missing attribute '{attribute}'"));
            return false;
        }

        value = nameAttribute.Value;
        return true;
    }

    protected virtual void OnParseError(XmlParseErrorEventArgs error)
    {
        ErrorReporter?.Report(ToString(), error);
    }
}