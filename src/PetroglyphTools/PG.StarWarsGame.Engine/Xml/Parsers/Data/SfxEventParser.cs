using System;
using System.Xml.Linq;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.Xml.Tags;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers.Data;

public sealed class SfxEventParser(
    IReadOnlyValueListDictionary<Crc32, SfxEvent> parsedElements,
    IServiceProvider serviceProvider,
    IXmlParserErrorListener? listener = null)
    : XmlObjectParser<SfxEvent>(parsedElements, serviceProvider, listener)
{
    protected override IPetroglyphXmlElementParser? GetParser(string tag)
    {
        switch (tag)
        {
            case SfxEventXmlTags.UsePreset:
            case SfxEventXmlTags.OverlapTest:
            case SfxEventXmlTags.ChainedSfxEvent:
                return PrimitiveParserProvider.StringParser;
            case SfxEventXmlTags.IsPreset:
            case SfxEventXmlTags.Is3D:
            case SfxEventXmlTags.Is2D:
            case SfxEventXmlTags.IsGui:
            case SfxEventXmlTags.IsHudVo:
            case SfxEventXmlTags.IsUnitResponseVo:
            case SfxEventXmlTags.IsAmbientVo:
            case SfxEventXmlTags.Localize:
            case SfxEventXmlTags.PlaySequentially:
            case SfxEventXmlTags.KillsPreviousObjectSFX:
                return PrimitiveParserProvider.BooleanParser;
            case SfxEventXmlTags.Samples:
            case SfxEventXmlTags.PreSamples:
            case SfxEventXmlTags.PostSamples:
            case SfxEventXmlTags.TextID:
                return PrimitiveParserProvider.LooseStringListParser;
            case SfxEventXmlTags.Priority:
            case SfxEventXmlTags.MinPitch:
            case SfxEventXmlTags.MaxPitch:
            case SfxEventXmlTags.MinPan2D:
            case SfxEventXmlTags.MaxPan2D:
            case SfxEventXmlTags.PlayCount:
            case SfxEventXmlTags.MaxInstances:
                return PrimitiveParserProvider.IntParser;
            case SfxEventXmlTags.Probability:
            case SfxEventXmlTags.MinVolume:
            case SfxEventXmlTags.MaxVolume:
                return PrimitiveParserProvider.Max100ByteParser;
            case SfxEventXmlTags.MinPredelay:
            case SfxEventXmlTags.MaxPredelay:
            case SfxEventXmlTags.MinPostdelay:
            case SfxEventXmlTags.MaxPostdelay:
                return PrimitiveParserProvider.UIntParser;
            case SfxEventXmlTags.LoopFadeInSeconds:
            case SfxEventXmlTags.LoopFadeOutSeconds:
            case SfxEventXmlTags.VolumeSaturationDistance:
                return PrimitiveParserProvider.FloatParser;
            default:
                return null;
        }
    }

    public override SfxEvent Parse(XElement element, out Crc32 nameCrc)
    {
        var name = GetNameAttributeValue(element);
        nameCrc = HashingService.GetCrc32Upper(name.AsSpan(), PGConstants.PGCrc32Encoding);

        var properties = ParseXmlElement(element);

        return new SfxEvent(name, nameCrc, properties, XmlLocationInfo.FromElement(element));
    }

    protected override bool OnParsed(XElement element, string tag, object value, ValueListDictionary<string, object?> properties, string? outerElementName)
    {
        if (tag == SfxEventXmlTags.UsePreset)
        {
            var presetName = value as string;
            var presetNameCrc = HashingService.GetCrc32Upper(presetName.AsSpan(), PGConstants.PGCrc32Encoding);
            if (presetNameCrc != default && ParsedElements.TryGetFirstValue(presetNameCrc, out var preset))
                CopySfxPreset(properties, preset);
            else
            {
                var location = XmlLocationInfo.FromElement(element);
                OnParseError(new XmlParseErrorEventArgs(location.XmlFile, element, XmlParseErrorKind.MissingReference,
                    $"Cannot to find preset '{presetName}' for SFXEvent '{outerElementName ?? "NONE"}'"));
            }
        }
        return true;
    }

    private static void CopySfxPreset(ValueListDictionary<string, object?> currentXmlProperties, SfxEvent preset)
    {
        /*
         * The engine also copies the Use_Preset *of* the preset, (which almost most cases is null)
         * As this would cause that the SfxEvent using the preset, would not have a reference to its original preset, we do not copy the preset
         * Example:
         *
         *  <SfxEvent Name="A">                         <SfxEvent Name="Preset">
         *      <Use_Preset>Preset</UsePreset>              <Is_Preset>Yes</Is_Preset>
         *  </SfxEvent>                                     <Min_Volume>90</Min_Volume>
         *                                              </SfxEvent>
         *
         * Engine Behavior:    SFXEvent instance(Name: A, Use_Preset: null, Min_Volume: 90)
         * PG.StarWarsGame.Engine Behavior: SFXEvent instance(Name: A, Use_Preset: Preset, Min_Volume: 90)
         */

        foreach (var keyValuePair in preset.XmlProperties)
        {
            if (keyValuePair.Key is SfxEventXmlTags.UsePreset or SfxEventXmlTags.IsPreset)
                continue;
            currentXmlProperties.Add(keyValuePair.Key, keyValuePair.Value);
        }

        currentXmlProperties.Add(SfxEventXmlTags.PresetXRef, preset);
    }

    public override SfxEvent Parse(XElement element) => throw new NotSupportedException();
}