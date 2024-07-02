using System;
using System.Collections.Generic;
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
        var files = new List<string>();
        foreach (var child in element.Elements())
        {
            if (child.Name == "File")
            {
                var file = PrimitiveParserProvider.StringParser.Parse(child);
                files.Add(file);
            }
        }
        return new XmlFileContainer(files);
    }
}