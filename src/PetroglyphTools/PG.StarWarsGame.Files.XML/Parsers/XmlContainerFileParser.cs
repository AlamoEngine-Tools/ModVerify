using AnakinRaW.CommonUtilities.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using System;
using System.IO;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class XmlContainerFileParser<T>(
    IServiceProvider serviceProvider,
    INamedXmlObjectParser<T> elementParser,
    IXmlParserErrorReporter? listener = null)
    : PetroglyphXmlFileParserBase(serviceProvider, listener), IXmlContainerFileParser<T> 
    where T : NamedXmlObject
{
    public INamedXmlObjectParser<T> ElementParser { get; } =
        elementParser ?? throw new ArgumentNullException(nameof(elementParser));

    public void ParseFile(Stream xmlStream, IFrugalValueListDictionary<Crc32, T> parsedEntries)
    {
        var root = GetRootElement(xmlStream, out _);
        foreach (var xElement in root.Elements()) 
            ParseElement(xElement, parsedEntries);
    }

    private void ParseElement(XElement element, IFrugalValueListDictionary<Crc32, T> parsedEntries)
    {
        var parsedElement = ElementParser.Parse(element, parsedEntries, out var entryCrc); 
        parsedEntries.Add(entryCrc, parsedElement);
    }
}