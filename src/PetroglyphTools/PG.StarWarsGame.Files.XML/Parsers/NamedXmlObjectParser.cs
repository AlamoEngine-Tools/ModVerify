using System;
using System.Text;
using System.Xml.Linq;
using AnakinRaW.CommonUtilities.Collections;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class NamedXmlObjectParser<TObject>(
    IServiceProvider serviceProvider,
    IXmlTagMapper<TObject> tagMapper,
    IXmlParserErrorReporter? errorReporter)
    : XmlObjectParserBase<TObject, IReadOnlyFrugalValueListDictionary<Crc32, TObject>>(tagMapper, errorReporter)
    where TObject : NamedXmlObject
{
    protected virtual bool UpperCaseNameForCrc => true;

    protected readonly ICrc32HashingService HashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();

    public TObject Parse(XElement element, IReadOnlyFrugalValueListDictionary<Crc32, TObject> parsedEntries, out Crc32 nameCrc)
    {
        var name = GetXmlObjectName(element, out nameCrc);
        var namedXmlObject = CreateXmlObject(name, nameCrc, element, XmlLocationInfo.FromElement(element));
        Parse(namedXmlObject, element, parsedEntries);
        ValidateValues(namedXmlObject, element);
        namedXmlObject.CoerceValues();
        return namedXmlObject;
    }

    protected abstract TObject CreateXmlObject(string name, Crc32 nameCrc, XElement element, XmlLocationInfo location);

    private string GetXmlObjectName(XElement element, out Crc32 crc32)
    {
        GetNameAttributeValue(element, out var name);
        crc32 = UpperCaseNameForCrc
            ? HashingService.GetCrc32Upper(name.AsSpan(), Encoding.ASCII)
            : HashingService.GetCrc32(name.AsSpan(), Encoding.ASCII);

        if (crc32 == default)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                Message = $"Name for XML object of type {typeof(TObject).Name} cannot be empty.",
                ErrorKind = XmlParseErrorKind.InvalidValue
            });
        }

        return name;
    }
}