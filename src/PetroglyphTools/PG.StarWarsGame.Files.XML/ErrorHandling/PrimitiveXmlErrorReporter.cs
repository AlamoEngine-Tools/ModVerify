using System;

namespace PG.StarWarsGame.Files.XML.ErrorHandling;

internal sealed class PrimitiveXmlErrorReporter : IXmlParserErrorReporter
{
    internal delegate void XmlErrorEventHandler(XmlError error);

    public event XmlErrorEventHandler? PrimitiveParseError;

    private static readonly Lazy<PrimitiveXmlErrorReporter> LazyInstance = new(() => new PrimitiveXmlErrorReporter(), true);

    public static PrimitiveXmlErrorReporter Instance => LazyInstance.Value;

    private PrimitiveXmlErrorReporter()
    {
    }

    public void Report(XmlError error)
    {
        PrimitiveParseError?.Invoke(error);
    }
}