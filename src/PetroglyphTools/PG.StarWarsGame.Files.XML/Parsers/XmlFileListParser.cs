using System;
using System.Collections.Generic;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers.Primitives;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class XmlFileListParser(IServiceProvider serviceProvider, IXmlParserErrorReporter? errorReporter = null) :
    PetroglyphXmlFileParser<XmlFileListContainer>(serviceProvider, errorReporter)
{
    protected override bool LoadLineInfo => false;

    protected override XmlFileListContainer Parse(XElement element, string fileName)
    {
        var files = new List<string>();
        foreach (var child in element.Elements())
        {
            if (child.Name == "File")
            {
                var file = PetroglyphXmlStringParser.Instance.Parse(child);
                files.Add(file);
            }
        }
        return new XmlFileListContainer(files);
    }
}