using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class XmlFileListParser(IServiceProvider serviceProvider, IXmlParserErrorReporter? errorReporter = null) :
    XmlFileParser<XmlFileList>(serviceProvider, errorReporter)
{ 
    protected override XmlFileList ParseRoot(XElement element, string fileName)
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
                    ErrorReporter?.Report(new XmlError(this, child)
                    {
                        ErrorKind = XmlParseErrorKind.InvalidValue,
                        Message = "Empty value in <File> tag",
                    });
                }
                files.Add(file);
            }
            else
            {
                ErrorReporter?.Report(new XmlError(this, child)
                {
                    ErrorKind = XmlParseErrorKind.UnknownNode,
                    Message = $"Tag '<{tagName}>' is not supported. Only '<File>' is supported.",
                });
            }
        }
        return new XmlFileList(new ReadOnlyCollection<string>(files), new XmlLocationInfo(fileName, null));
    }
}