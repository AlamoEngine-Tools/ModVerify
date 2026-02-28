using System;
using System.Collections.Generic;
using System.Xml.Linq;
using AnakinRaW.CommonUtilities;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML.Data;

namespace PG.StarWarsGame.Files.XML.Parsers;

public abstract class XmlTagMapper<TObject> : IXmlTagMapper<TObject> where TObject : XmlObject
{
    private delegate void ParserValueAction(TObject target, XElement element, bool replace);

    private readonly Dictionary<Crc32, ParserValueAction> _tagMappings = new();
    private readonly ICrc32HashingService _crcService;

    protected XmlTagMapper(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));
        _crcService = serviceProvider.GetRequiredService<ICrc32HashingService>();

        // ReSharper disable once VirtualMemberCallInConstructor
        BuildMappings();
    }

    protected abstract void BuildMappings();

    protected static void SetOrReplaceList<T>(IList<T> destinationList, IEnumerable<T> values, bool replace)
    {
        if (replace)
            destinationList.Clear();
        foreach (var value in values)
            destinationList.Add(value);
    }

    protected void AddMapping<TValue>(string tagName, Func<XElement, TValue> parser,
        Action<TObject, TValue> setter)
    {
        AddMapping(tagName, parser, (target, value, _) => setter(target, value));
    }

    protected void AddMapping<TValue>(string tagName, Func<XElement, TValue> parser, Action<TObject, TValue, bool> setter)
    {
        ThrowHelper.ThrowIfNullOrEmpty(tagName);
        if (tagName.Length >= XmlFileConstants.MaxTagNameLength)
            throw new ArgumentOutOfRangeException(
                $"Tag name '{tagName}' exceeds maximum length of {XmlFileConstants.MaxTagNameLength} characters", nameof(tagName));

        if (parser == null)
            throw new ArgumentNullException(nameof(parser));
        if (setter == null)
            throw new ArgumentNullException(nameof(setter));

        var crc = GetCrc32(tagName);

        _tagMappings[crc] = (target, element, replace) =>
        {
            var value = parser(element);
            setter(target, value, replace);
        };
    }

    public bool TryParseEntry(XElement element, TObject target, bool replace)
    {
        var tagName = element.Name.LocalName;
        if (tagName.Length >= XmlFileConstants.MaxTagNameLength)
            return false;

        var crc = GetCrc32(tagName);

        if (!_tagMappings.TryGetValue(crc, out var mapping))
            return false;

        mapping(target, element, replace);
        return true;
    }

    private Crc32 GetCrc32(string tagName)
    {
        return _crcService.GetCrc32Upper(tagName, XmlFileConstants.XmlEncoding);
    }
}