using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using System;
using System.IO;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class XmlFileParser<T>(IServiceProvider serviceProvider, IXmlParserErrorReporter? errorReporter = null) 
    : PetroglyphXmlFileParserBase(serviceProvider, errorReporter) where T : XmlObject
{
    public T ParseFile(Stream xmlStream)
    {
        var root = GetRootElement(xmlStream, out var fileName);
        return ParseRoot(root, fileName);
    }

    protected abstract T ParseRoot(XElement element, string fileName);
}