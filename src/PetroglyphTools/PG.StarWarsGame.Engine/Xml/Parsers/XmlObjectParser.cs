using System;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

public abstract class XmlObjectParser<T>(IReadOnlyValueListDictionary<Crc32, T> parsedElements, IServiceProvider serviceProvider, IXmlParserErrorListener? listener = null)
    : PetroglyphXmlElementParser<T>(serviceProvider, listener) where T : XmlObject
{ 
    protected IReadOnlyValueListDictionary<Crc32, T> ParsedElements { get; } = parsedElements ?? throw new ArgumentNullException(nameof(parsedElements));

    protected ICrc32HashingService HashingService { get; } = serviceProvider.GetRequiredService<ICrc32HashingService>();

    public abstract T Parse(XElement element, out Crc32 nameCrc);

    protected abstract IPetroglyphXmlElementParser? GetParser(string tag);

    protected ValueListDictionary<string, object?> ParseXmlElement(XElement element, string? name = null)
    {
        var xmlProperties = new ValueListDictionary<string, object?>();
        foreach (var elm in element.Elements())
        {
            var tagName = elm.Name.LocalName;

            var parser = GetParser(tagName);

            if (parser is null)
            {
                // TODO
                //var nameOrPosition = name ?? XmlLocationInfo.FromElement(element).ToString();
                //Logger?.LogWarning($"Unable to find parser for tag '{tagName}' in element '{nameOrPosition}'");
                continue;
            }
            
            var value = parser.Parse(elm);
            
            if (OnParsed(elm, tagName, value, xmlProperties, name))
                xmlProperties.Add(tagName, value);
        }
        return xmlProperties;
    }

    protected virtual bool OnParsed(XElement element, string tag, object value, ValueListDictionary<string, object?> properties, string? outerElementName)
    {
        return true;
    }
}