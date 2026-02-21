using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using System.Xml.Linq;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class XmlObjectParser<TObject>(IXmlTagMapper<TObject> tagMapper, IXmlParserErrorReporter? errorReporter = null)
    : XmlObjectParserBase<TObject, EmptyParseState>(tagMapper, errorReporter)
    where TObject : XmlObject
{
    public TObject Parse(XElement element)
    {
        var xmlObject = CreateXmlObject(XmlLocationInfo.FromElement(element));
        Parse(xmlObject, element, EmptyParseState.Instance);
        ValidateAndFixupValues(xmlObject, element); ;
        return xmlObject;
    }

    protected abstract TObject CreateXmlObject(XmlLocationInfo location);

    protected sealed override bool ParseTag(XElement tag, TObject xmlObject, in EmptyParseState parseState)
    {
        return ParseTag(tag, xmlObject);
    }

    protected virtual bool ParseTag(XElement tag, TObject xmlObject)
    {
        return XmlTagMapper.TryParseEntry(tag, xmlObject);
    }
}