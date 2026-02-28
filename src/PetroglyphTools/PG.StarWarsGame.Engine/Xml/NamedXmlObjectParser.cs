using System;
using System.Xml.Linq;
using AnakinRaW.CommonUtilities.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml;

public abstract class NamedXmlObjectParser<T> : 
    XmlObjectParserBase<T, IReadOnlyFrugalValueListDictionary<Crc32, T>>, INamedXmlObjectParser<T>
    where T : NamedXmlObject
{
    protected abstract bool UpperCaseNameForCrc { get; }
    protected abstract bool UpperCaseNameForObject { get; }

    protected readonly ICrc32HashingService HashingService;
    
    protected readonly ILogger? Logger;

    protected NamedXmlObjectParser(
        GameEngineType engine,
        XmlTagMapper<T> tagMapper,
        IXmlParserErrorReporter? errorReporter,
        IServiceProvider serviceProvider) : base(engine, tagMapper, errorReporter)
    {
        HashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public T Parse(XElement element, IReadOnlyFrugalValueListDictionary<Crc32, T> parsedEntries, out Crc32 nameCrc)
    {
        var name = GetXmlObjectName(element, out nameCrc);
        var namedXmlObject = CreateXmlObject(name, nameCrc, element, parsedEntries, XmlLocationInfo.FromElement(element));
        ParseObject(namedXmlObject, element, false, parsedEntries);
        ValidateAndFixupValues(namedXmlObject, element, parsedEntries);
        return namedXmlObject;
    }

    protected abstract T CreateXmlObject(
        string name, 
        Crc32 nameCrc, 
        XElement element,
        IReadOnlyFrugalValueListDictionary<Crc32, T> parsedEntries,
        XmlLocationInfo location);

    protected virtual Crc32 CreateNameCrc(string name)
    {
        return UpperCaseNameForCrc
            ? HashingService.GetCrc32Upper(name.AsSpan(), XmlFileConstants.XmlEncoding)
            : HashingService.GetCrc32(name.AsSpan(), XmlFileConstants.XmlEncoding);
    }

    protected string GetXmlObjectName(XElement element, out Crc32 crc32)
    {
        GetNameAttributeValue(element, out var name, UpperCaseNameForObject);
        crc32 = CreateNameCrc(name);

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