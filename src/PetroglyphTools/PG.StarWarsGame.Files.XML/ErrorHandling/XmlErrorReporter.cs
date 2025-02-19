using AnakinRaW.CommonUtilities;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Files.XML.ErrorHandling;

public class XmlErrorReporter : DisposableObject, IXmlParserErrorReporter, IXmlParserErrorProvider
{
    public event XmlErrorEventHandler? XmlParseError;

    public XmlErrorReporter()
    {
        PrimitiveXmlErrorReporter.Instance.XmlParseError += OnPrimitiveError;
    }

    public virtual void Report(IPetroglyphXmlParser parser, XmlParseErrorEventArgs error)
    {
        XmlParseError?.Invoke(parser, error);
    }

    protected override void DisposeResources()
    {
        PrimitiveXmlErrorReporter.Instance.XmlParseError -= OnPrimitiveError;
    }

    private void OnPrimitiveError(IPetroglyphXmlParser parser, XmlParseErrorEventArgs error)
    {
        Report(parser, error);
    }
}