using System;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Files.XML.ErrorHandling;

internal sealed class PrimitiveXmlErrorReporter : IXmlParserErrorReporter, IXmlParserErrorProvider
{
    public event XmlErrorEventHandler? XmlParseError;

    private static readonly Lazy<PrimitiveXmlErrorReporter> LazyInstance = new(() => new PrimitiveXmlErrorReporter());

    public static PrimitiveXmlErrorReporter Instance => LazyInstance.Value;

    private PrimitiveXmlErrorReporter()
    {
    }

    public void Report(IPetroglyphXmlParser parser, XmlParseErrorEventArgs error)
    {
        XmlParseError?.Invoke(parser, error);
    }
}