using System;
using System.IO;
using System.Xml.Linq;
using PG.Commons.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class PetroglyphXmlFileContainerParser<T>(IServiceProvider serviceProvider, IXmlParserErrorReporter? listener = null)
    : PetroglyphXmlFileParserBase(serviceProvider, listener), IPetroglyphXmlFileContainerParser<T> where T : notnull
{
    public void ParseFile(Stream xmlStream, IValueListDictionary<Crc32, T> parsedEntries)
    {
        var root = GetRootElement(xmlStream, out var fileName);
        if (root is not null)
            Parse(root, parsedEntries, fileName);
    }

    protected abstract void Parse(XElement element, IValueListDictionary<Crc32, T> parsedElements, string fileName);
}