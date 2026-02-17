using AnakinRaW.CommonUtilities;

namespace PG.StarWarsGame.Files.XML.ErrorHandling;

public class XmlErrorReporter : DisposableObject, IXmlParserErrorReporter
{
    public XmlErrorReporter()
    {
        PrimitiveXmlErrorReporter.Instance.PrimitiveParseError += OnPrimitiveError;
    }

    public virtual void Report(XmlError error)
    {
    }

    protected override void DisposeResources()
    {
        PrimitiveXmlErrorReporter.Instance.PrimitiveParseError -= OnPrimitiveError;
    }

    private void OnPrimitiveError(XmlError error)
    {
        Report(error);
    }
}