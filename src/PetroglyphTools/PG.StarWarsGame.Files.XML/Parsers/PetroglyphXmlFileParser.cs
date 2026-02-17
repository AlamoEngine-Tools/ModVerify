using System;
using System.IO;
using System.Xml.Linq;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlFileParser<T>(IServiceProvider serviceProvider, IXmlParserErrorReporter? errorReporter = null) 
    : PetroglyphXmlFileParserBase(serviceProvider, errorReporter), IPetroglyphXmlFileParser<T> where T : notnull
{
    public T ParseFile(Stream xmlStream)
    {
        var root = GetRootElement(xmlStream, out var fileName);
        return Parse(root, fileName);
    }

    protected abstract T Parse(XElement element, string fileName);
}