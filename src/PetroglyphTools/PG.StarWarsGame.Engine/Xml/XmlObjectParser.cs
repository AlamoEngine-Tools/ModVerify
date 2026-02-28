using System.Xml.Linq;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.Xml;

public abstract class XmlObjectParser<TObject>(
    GameEngineType engine,
    XmlTagMapper<TObject> tagMapper,
    IXmlParserErrorReporter? errorReporter = null)
    : XmlObjectParserBase<TObject, XmlObjectParser<TObject>.EmptyParseState>(engine, tagMapper, errorReporter)
    where TObject : XmlObject
{
    public TObject Parse(XElement element)
    {
        var xmlObject = CreateXmlObject(XmlLocationInfo.FromElement(element));
        ParseObject(xmlObject, element, false, EmptyParseState.Instance);
        ValidateAndFixupValues(xmlObject, element, EmptyParseState.Instance);
        return xmlObject;
    }

    protected abstract TObject CreateXmlObject(XmlLocationInfo location);

    protected sealed override bool ParseTag(XElement tag, TObject xmlObject, bool replace, in EmptyParseState parseState)
    {
        return ParseTag(tag, xmlObject, replace);
    }
    
    protected sealed override void ValidateAndFixupValues(TObject xmlObject, XElement element, in EmptyParseState parseState)
    {
        ValidateAndFixupValues(xmlObject, element);
    }

    protected virtual bool ParseTag(XElement tag, TObject xmlObject, bool replace)
    {
        return XmlTagMapper.TryParseEntry(tag, xmlObject, replace, Engine);
    }

    protected virtual void ValidateAndFixupValues(TObject xmlObject, XElement element)
    {
    }

    public readonly struct EmptyParseState
    {
        public static readonly EmptyParseState Instance = new();
    }
}