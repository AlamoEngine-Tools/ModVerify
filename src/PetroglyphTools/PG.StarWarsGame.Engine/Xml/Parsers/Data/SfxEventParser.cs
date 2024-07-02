using System;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers.Data;

public static class SfxEventXmlTags
{
    public const string IsPreset = "Is_Preset";
    public const string UsePreset = "Use_Preset";
    public const string Samples = "Samples";
    public const string PreSamples = "Pre_Samples";
    public const string PostSamples = "Post_Samples";
    public const string TextID = "Text_ID";
    public const string PlaySequentially = "Play_Sequentially";
    public const string Priority = "Priority";
    public const string Probability = "Probability";
    public const string PlayCount = "Play_Count";
    public const string LoopFadeInSeconds = "Loop_Fade_In_Seconds";
    public const string LoopFadeOutSeconds = "Loop_Fade_Out_Seconds";
    public const string MaxInstances = "Max_Instances";
    public const string MinVolume = "Min_Volume";
    public const string MaxVolume = "Max_Volume";
    public const string MinPitch = "Min_Pitch";
    public const string MaxPitch = "Max_Pitch";
    public const string MinPan2D = "Min_Pan2D";
    public const string MaxPan2D = "Max_Pan2D";
    public const string MinPredelay = "Min_Predelay";
    public const string MaxPredelay = "Max_Predelay";
    public const string MinPostdelay = "Min_Postdelay";
    public const string MaxPostdelay = "Max_Postdelay";
    public const string VolumeSaturationDistance = "Volume_Saturation_Distance";
    public const string KillsPreviousObjectSFX = "Kills_Previous_Object_SFX";
    public const string OverlapTest = "Overlap_Test";
    public const string Localize = "Localize";
    public const string Is2D = "Is_2D";
    public const string Is3D = "Is_3D";
    public const string IsGui = "Is_GUI";
    public const string IsHudVo = "Is_HUD_VO";
    public const string IsUnitResponseVo = "Is_Unit_Response_VO";
    public const string IsAmbientVo = "Is_Ambient_VO";
    public const string ChainedSfxEvent = "Chained_SFXEvent";
}

public sealed class SfxEventParser(
    IReadOnlyValueListDictionary<Crc32, SfxEvent> parsedElements,
    IServiceProvider serviceProvider) 
    : XmlObjectParser<SfxEvent>(parsedElements, serviceProvider)
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
                return PrimitiveParserProvider.ByteParser;
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

    public override SfxEvent Parse(
        XElement element, 
        out Crc32 nameCrc)
    {
        var name = GetNameAttributeValue(element);
        nameCrc = HashingService.GetCrc32Upper(name.AsSpan(), PGConstants.PGCrc32Encoding);

        var properties = ParseXmlElement(element);

        return new SfxEvent(name, nameCrc, properties, XmlLocationInfo.FromElement(element));
    }

    protected override bool OnParsed(string tag, object value, ValueListDictionary<string, object?> properties, string? elementName)
    {
        if (tag == SfxEventXmlTags.UsePreset)
        {
            var presetName = value as string;
            var presetNameCrc = HashingService.GetCrc32Upper(presetName.AsSpan(), PGConstants.PGCrc32Encoding);
            if (presetNameCrc == default || !ParsedElements.TryGetFirstValue(presetNameCrc, out var preset))
            {
                Logger?.LogWarning($"Cannot to find preset '{presetName}' for SFXEvent '{elementName ?? "NONE"}'");
                return false;
            }
            CopySfxPreset(properties, preset);
        }

        return true;
    }


    private void CopySfxPreset(ValueListDictionary<string, object?> currentXmlProperties, SfxEvent preset)
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
    }


    public override SfxEvent Parse(XElement element) => throw new NotSupportedException();
}