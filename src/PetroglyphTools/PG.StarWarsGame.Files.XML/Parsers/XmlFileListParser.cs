using System;
using System.Collections.Generic;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;

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
            var tagName = GetTagName(child);
            if (tagName == "File")
            {
                var file = PetroglyphXmlStringParser.Instance.Parse(child);
                if (file.Length == 0)
                {
                    ErrorReporter?.Report(this, 
                        new XmlParseErrorEventArgs(element, XmlParseErrorKind.InvalidValue, "Empty value in <File> tag."));
                }
                files.Add(file);
            }
            else
            {
                ErrorReporter?.Report(this, new XmlParseErrorEventArgs(child, XmlParseErrorKind.UnknownNode, 
                        $"Tag '<{tagName}>' is not supported. Only '<File>' is supported."));
            }
        }
        return new XmlFileListContainer(files);
    }
}