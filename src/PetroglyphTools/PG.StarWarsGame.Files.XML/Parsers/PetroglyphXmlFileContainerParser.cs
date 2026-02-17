using AnakinRaW.CommonUtilities.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using System;
using System.IO;

namespace PG.StarWarsGame.Files.XML.Parsers;

public sealed class PetroglyphXmlFileContainerParser<T>(
    IServiceProvider serviceProvider,
    IPetroglyphXmlNamedElementParser<T> elementParser,
    IXmlParserErrorReporter? listener = null)
    : PetroglyphXmlFileParserBase(serviceProvider, listener), IPetroglyphXmlFileContainerParser<T> where T : notnull
{
    public IPetroglyphXmlNamedElementParser<T> ElementParser { get; } =
        elementParser ?? throw new ArgumentNullException(nameof(elementParser));

    public void ParseFile(Stream xmlStream, IFrugalValueListDictionary<Crc32, T> parsedEntries)
    {
        var root = GetRootElement(xmlStream, out _);
        
        if (!root.HasElements)
        {
            OnParseError(XmlParseErrorEventArgs.FromEmptyRoot(root));
            return;
        }

        foreach (var xElement in root.Elements())
        {
            var parsedElement = ElementParser.Parse(xElement, parsedEntries, out var entryCrc);
            parsedEntries.Add(entryCrc, parsedElement);
        }
    }
}