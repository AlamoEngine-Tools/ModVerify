using System.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlParserBase : IPetroglyphXmlParserInfo
{
    protected readonly IXmlParserErrorReporter? ErrorReporter;

    public string Name { get; }

    public override string ToString()
    {
        return Name;
    }

    protected bool IsTagValid(XElement element)
    {
        var tagName = element.Name.LocalName;
        if (string.IsNullOrEmpty(tagName))
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.EmptyNodeName,
                Message = "A tag name cannot be null or empty.",
            });
            return false;
        }

        if (tagName.Length > XmlFileConstants.MaxTagNameLength)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.TooLongData,
                Message = $"A tag name can be only {XmlFileConstants.MaxTagNameLength} chars long.",
            });
            return false;
        }

        return true;
    }

    protected PetroglyphXmlParserBase(IXmlParserErrorReporter? errorReporter)
    {
        Name = GetType().FullName!;
        ErrorReporter = errorReporter;
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
        // In this engine, this is actually case-sensitive
        var nameAttribute = element.Attributes()
            .FirstOrDefault(a => a.Name.LocalName == attribute);
        
        if (nameAttribute is null)
        {
            value = defaultValue;
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.MissingAttribute,
                Message = $"Missing attribute '{attribute}'",
            });
            return false;
        }

        value = nameAttribute.Value;
        return true;
    }
}