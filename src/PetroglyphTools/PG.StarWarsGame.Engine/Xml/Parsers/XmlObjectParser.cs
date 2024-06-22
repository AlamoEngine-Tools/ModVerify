using System;
using System.Xml.Linq;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

public abstract class XmlObjectParser<T>(IServiceProvider serviceProvider) : PetroglyphXmlElementParser<T> where T : XmlObject
{
    protected IServiceProvider ServiceProvider { get; } = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    protected abstract bool IsTagSupported(string tag);

    protected IPetroglyphXmlElementParser? GetParser(string tag)
    {
        if (!IsTagSupported(tag))
            return null;

        return null;
    }

    protected ValueListDictionary<string, object> ToKeyValuePairList(XElement element)
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
}

internal interface IXmlParserFactory
{

}