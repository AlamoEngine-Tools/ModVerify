using System;
using System.Collections.Generic;
using System.Xml.Linq;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers.Primitives;

public class XmlFileContainerParser(IServiceProvider serviceProvider, IXmlParserErrorListener? listener = null) : 
    PetroglyphXmlFileParser<XmlFileContainer>(serviceProvider, listener)
{
    protected override bool LoadLineInfo => false;

    protected override void Parse(XElement element, IValueListDictionary<Crc32, XmlFileContainer> parsedElements, string fileName)
    {
        throw new NotSupportedException();
    }

    protected override XmlFileContainer Parse(XElement element, string fileNaem)
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