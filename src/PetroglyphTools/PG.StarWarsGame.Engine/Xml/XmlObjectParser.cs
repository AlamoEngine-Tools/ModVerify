using AnakinRaW.CommonUtilities.Collections;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;
using System;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons.Hashing;
using Crc32 = PG.Commons.Hashing.Crc32;

namespace PG.StarWarsGame.Engine.Xml;

public abstract class XmlObjectParserBase<TObject, TParseState>(IXmlTagMapper<TObject> tagMapper, IXmlParserErrorReporter? errorReporter)
    : PetroglyphXmlParserBase(errorReporter) where TObject : XmlObject
{
    protected readonly IXmlTagMapper<TObject> TagMapper = tagMapper ?? throw new ArgumentNullException(nameof(tagMapper));

    protected virtual void ValidateValues(TObject namedXmlObject, XElement element)
    {
    }

    protected void Parse(TObject xmlObject, XElement element, in TParseState state)
    {
        foreach (var tag in element.Elements())
        {
            if (!ParseTag(tag, xmlObject, state))
            {
                ErrorReporter?.Report(new XmlError(this, element)
                {
                    Message = $"The node '{tag.Name}' is not supported.",
                    ErrorKind = XmlParseErrorKind.UnknownNode
                });
            }
        }
    }

    protected abstract bool ParseTag(XElement tag, TObject xmlObject, in TParseState parseState);
}


public abstract class NamedXmlObjectParser<TObject>(
    IServiceProvider serviceProvider,
    IXmlTagMapper<TObject> tagMapper,
    IXmlParserErrorReporter? errorReporter)
    : XmlObjectParserBase<TObject, IReadOnlyFrugalValueListDictionary<Crc32, TObject>>(tagMapper, errorReporter), 
        IPetroglyphXmlNamedElementParser<TObject>
    where TObject : NamedXmlObject
{
    protected readonly ICrc32HashingService HashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();

    public TObject Parse(XElement element, IReadOnlyFrugalValueListDictionary<Crc32, TObject> parsedEntries, out Crc32 nameCrc)
    {
        var name = GetXmlObjectName(element, true, out nameCrc);
        var namedXmlObject = CreateXmlObject(name, nameCrc, element, XmlLocationInfo.FromElement(element));
        Parse(namedXmlObject, element, parsedEntries);
        ValidateValues(namedXmlObject, element);
        namedXmlObject.CoerceValues();
        return namedXmlObject;
    }

    protected abstract TObject CreateXmlObject(string name, Crc32 nameCrc, XElement element, XmlLocationInfo location);

    protected string GetXmlObjectName(XElement element, bool uppercaseName, out Crc32 crc32)
    {
        GetNameAttributeValue(element, out var name);
        crc32 = uppercaseName
            ? HashingService.GetCrc32Upper(name.AsSpan(), Encoding.ASCII)
            : HashingService.GetCrc32(name.AsSpan(), Encoding.ASCII);

        if (crc32 == default)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                Message = "Name for XmlObject cannot be empty.",
                ErrorKind = XmlParseErrorKind.InvalidValue
            });
        }

        return name;
    }
}

public abstract class XmlObjectParser<TObject>(IXmlTagMapper<TObject> tagMapper, IXmlParserErrorReporter? errorReporter = null)
    : XmlObjectParserBase<TObject, EmptyParseState>(tagMapper, errorReporter), IPetroglyphXmlElementParser<TObject>
    where TObject : XmlObject
{
    public TObject Parse(XElement element)
    {
        var xmlObject = CreateXmlObject(XmlLocationInfo.FromElement(element));
        Parse(xmlObject, element, EmptyParseState.Instance);
        ValidateValues(xmlObject, element);
        xmlObject.CoerceValues();
        return xmlObject;
    }

    protected abstract TObject CreateXmlObject(XmlLocationInfo location);

    protected sealed override bool ParseTag(XElement tag, TObject xmlObject, in EmptyParseState parseState)
    {
        return ParseTag(tag, xmlObject);
    }

    protected abstract bool ParseTag(XElement tag, TObject xmlObject);
}

public readonly struct EmptyParseState
{
    public static readonly EmptyParseState Instance = new();
}