using System;
using System.Linq;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public class XmlFileContainerParser(IServiceProvider serviceProvider) : PetroglyphXmlFileParser<XmlFileContainer>(serviceProvider)
{
    protected override IPetroglyphXmlElementParser? GetParser(string tag)
    {
        if (tag == "File")
            return PetroglyphXmlStringParser.Instance;
        return null;
    }

    public override XmlFileContainer Parse(XElement element)
    {
        var xmlValues = ToKeyValuePairList(element);

        return xmlValues.TryGetValues("File", out var files)
            ? new XmlFileContainer(files.OfType<string>().ToList())
            : new XmlFileContainer([]);
    }
}