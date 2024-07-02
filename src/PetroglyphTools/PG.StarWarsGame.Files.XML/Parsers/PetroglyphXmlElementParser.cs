using System;
using System.Linq;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlElementParser<T>(IServiceProvider serviceProvider) 
    : PetroglyphXmlParser<T>(serviceProvider)
{
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
}