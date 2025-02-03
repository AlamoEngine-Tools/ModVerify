using Microsoft.Extensions.Logging;
using System;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers.Primitives;
using System.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlParser<T> : IPetroglyphXmlParser<T>
{
    private readonly IXmlParserErrorListener? _errorListener;

    protected IServiceProvider ServiceProvider { get; }

    protected ILogger? Logger { get; }

    protected IPrimitiveParserProvider PrimitiveParserProvider { get; }

    protected PetroglyphXmlParser(IServiceProvider serviceProvider, IXmlParserErrorListener? errorListener = null)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        PrimitiveParserProvider = serviceProvider.GetRequiredService<IPrimitiveParserProvider>();
        _errorListener = errorListener;
    }

    public abstract T Parse(XElement element);

    protected virtual void OnParseError(XmlParseErrorEventArgs e)
    {
        _errorListener?.OnXmlParseError(this, e);
    }

    protected string GetTagName(XElement element)
    {
        return element.Name.LocalName;
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

    object IPetroglyphXmlParser.Parse(XElement element)
    {
        return Parse(element)!;
    }
}