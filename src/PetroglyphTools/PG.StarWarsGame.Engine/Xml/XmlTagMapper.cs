using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using AnakinRaW.CommonUtilities;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Data;

namespace PG.StarWarsGame.Engine.Xml;

public abstract class XmlTagMapper<TObject> where TObject : XmlObject
{
    private delegate void ParserValueAction(TObject target, XElement element, bool replace);

    private readonly struct MappingEntry(SupportedEngines supportedEngines, ParserValueAction action)
    {
        public readonly SupportedEngines SupportedEngines = supportedEngines;
        public readonly ParserValueAction Action = action;
    }

    [Flags]
    public enum SupportedEngines
    {
        Eaw = 1,
        Foc = 2,
        All = Eaw | Foc
    }

    private readonly Dictionary<Crc32, MappingEntry> _tagMappings = new();
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

    protected void AddMapping<TValue>(
        string tagName, 
        Func<XElement, TValue> parser,
        Action<TObject, TValue> setter,
        SupportedEngines supportedEngines = SupportedEngines.All)
    {
        AddMapping(tagName, parser, (target, value, _) => setter(target, value), supportedEngines);
    }

    protected void AddMapping<TValue>(
        string tagName, 
        Func<XElement, TValue> parser, 
        Action<TObject, TValue, bool> setter, 
        SupportedEngines supportedEngines = SupportedEngines.All)
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

        _tagMappings[crc] = new MappingEntry(supportedEngines, (target, element, replace) =>
        {
            var value = parser(element);
            setter(target, value, replace);
        });
    }

    public bool TryParseEntry(XElement element, TObject target, bool replace, GameEngineType engine)
    {
        var tagName = element.Name.LocalName;
        if (tagName.Length >= XmlFileConstants.MaxTagNameLength)
            return false;

        var crc = GetCrc32(tagName);

        if (!_tagMappings.TryGetValue(crc, out var mapping))
            return false;

        if (!IsEngineSupported(mapping.SupportedEngines, engine))
            return false;

        mapping.Action(target, element, replace);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsEngineSupported(SupportedEngines supportedEngines, GameEngineType requestedEngine)
    {
        // Convert enum value to its corresponding flag by shifting bit 1 left
        // Eaw (0) -> 1 << 0 = 1, Foc (1) -> 1 << 1 = 2
        var engineFlag = (SupportedEngines)(1 << (int)requestedEngine);
        // Use bitwise AND to check if the flag is set in supportedEngines
        // Returns true if the bit is present, false otherwise
        return (supportedEngines & engineFlag) != 0;
    }

    private Crc32 GetCrc32(string tagName)
    {
        return _crcService.GetCrc32Upper(tagName, XmlFileConstants.XmlEncoding);
    }
}