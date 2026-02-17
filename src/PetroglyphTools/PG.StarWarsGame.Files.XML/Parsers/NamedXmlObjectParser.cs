using System;
using System.Text;
using System.Xml.Linq;
using AnakinRaW.CommonUtilities.Collections;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class NamedXmlObjectParser<T>(
    IServiceProvider serviceProvider,
    IXmlTagMapper<T> tagMapper,
    IXmlParserErrorReporter? errorReporter)
    : XmlObjectParserBase<T, IReadOnlyFrugalValueListDictionary<Crc32, T>>(tagMapper, errorReporter)
    where T : NamedXmlObject
{
    protected virtual bool UpperCaseNameForCrc => true;

    protected readonly ICrc32HashingService HashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();

    public T Parse(XElement element, IReadOnlyFrugalValueListDictionary<Crc32, T> parsedEntries, out Crc32 nameCrc)
    {
        var name = GetXmlObjectName(element, out nameCrc);
        var namedXmlObject = CreateXmlObject(name, nameCrc, element, XmlLocationInfo.FromElement(element));
        Parse(namedXmlObject, element, parsedEntries);
        ValidateValues(namedXmlObject, element);
        namedXmlObject.CoerceValues();
        return namedXmlObject;
    }

    protected abstract T CreateXmlObject(string name, Crc32 nameCrc, XElement element, XmlLocationInfo location);

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
                Message = $"Name for XML object of type {typeof(T).Name} cannot be empty.",
                ErrorKind = XmlParseErrorKind.InvalidValue
            });
        }

        return name;
    }
}