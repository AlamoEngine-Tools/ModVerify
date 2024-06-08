using System;
using System.Linq;
using System.Xml.Linq;
using PG.Commons.Hashing;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public class XmlFileContainerParser(IServiceProvider serviceProvider) : PetroglyphXmlFileParser<XmlFileContainer>(serviceProvider)
{
    protected override bool LoadLineInfo => false;

    protected override void Parse(XElement element, IValueListDictionary<Crc32, XmlFileContainer> parsedElements)
    {
        throw new NotSupportedException();
    }

    public override XmlFileContainer Parse(XElement element)
    {
        var xmlValues = ToKeyValuePairList(element);

        return xmlValues.TryGetValues("File", out var files)
            ? new XmlFileContainer(files.OfType<string>().ToList())
            : new XmlFileContainer([]);
    }
    protected override IPetroglyphXmlElementParser? GetParser(string tag)
    {
        return tag == "File" ? PetroglyphXmlStringParser.Instance : null;
    }

}