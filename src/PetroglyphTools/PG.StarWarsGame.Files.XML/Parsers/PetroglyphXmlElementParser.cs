using System;
using System.Linq;
using System.Xml.Linq;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML.Parsers.Primitives;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlElementParser<T>(IServiceProvider serviceProvider) : IPetroglyphXmlElementParser<T>
{
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;


    protected virtual IPetroglyphXmlElementParser? GetParser(string tag)
    {
        return PetroglyphXmlStringParser.Instance;
    }

    public abstract T Parse(XElement element);

    public abstract T Parse(XElement element, IReadOnlyValueListDictionary<Crc32, T> parsedElements, out Crc32 nameCrc);

    public ValueListDictionary<string, object> ToKeyValuePairList(XElement element)
    {
        var keyValuePairList = new ValueListDictionary<string, object>();
        foreach (var elm in element.Elements())
        {
            var tagName = elm.Name.LocalName;

            var parser = GetParser(tagName);

            if (parser is not null)
            {
                var value = parser.Parse(elm);
                keyValuePairList.Add(tagName, value);
            }
        }
        return keyValuePairList;
    }

    protected string GetNameAttributeValue(XElement element)
    {
        var nameAttribute = element.Attributes()
            .FirstOrDefault(a => a.Name.LocalName == "Name");
        return nameAttribute is null ? string.Empty : nameAttribute.Value;
    }

    object? IPetroglyphXmlElementParser.Parse(XElement element)
    {
        return Parse(element);
    }
}