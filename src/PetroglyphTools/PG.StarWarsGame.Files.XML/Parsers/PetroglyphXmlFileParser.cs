using System;
using System.IO;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlFileParser<T>(IServiceProvider serviceProvider, IXmlParserErrorReporter? errorReporter = null) 
    : PetroglyphXmlFileParserBase(serviceProvider, errorReporter), IPetroglyphXmlFileParser<T> where T : notnull
{
    public T? ParseFile(Stream xmlStream)
    {
        var root = GetRootElement(xmlStream, out var fileName);
        if (root is null)
        {
            var location = new XmlLocationInfo(fileName, 0);
            OnParseError(new XmlParseErrorEventArgs(location, XmlParseErrorKind.EmptyRoot,
                "Unable to get root node from XML file."));
        }
        return root is null ? default : Parse(root, fileName);
    }

    protected abstract T Parse(XElement element, string fileName);
}