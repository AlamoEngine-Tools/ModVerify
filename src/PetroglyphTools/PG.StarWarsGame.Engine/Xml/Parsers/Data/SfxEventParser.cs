using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.Audio.Sfx;
using PG.StarWarsGame.Engine.Xml.Tags;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.Xml.Parsers.Data;

public sealed class SfxEventParser(
    IReadOnlyValueListDictionary<Crc32, SfxEvent> parsedElements,
    IServiceProvider serviceProvider,
    IXmlParserErrorListener? listener = null)
    : XmlObjectParser<SfxEvent>(parsedElements, serviceProvider, listener)
{ 
    public override SfxEvent Parse(XElement element, out Crc32 nameCrc)
    {
        var name = GetXmlObjectName(element, out nameCrc);
        var sfxEvent = new SfxEvent(name, nameCrc, XmlLocationInfo.FromElement(element));
        Parse(sfxEvent, element, default);
        ValidateValues(sfxEvent, element);
        sfxEvent.CoerceValues();
        return sfxEvent;
    }

    private void ValidateValues(SfxEvent sfxEvent, XElement element)
    {
        if (sfxEvent.Name.Length > PGConstants.MaxSFXEventName)
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.TooLongData,
                $"SFXEvent name '{sfxEvent.Name}' is too long."));
        }

        if (sfxEvent is { Is2D: true, Is3D: true })
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.InvalidValue,
                $"SFXEvent '{sfxEvent.Name}' is defined as 2D and 3D."));
        }

        if (sfxEvent.MinVolume > sfxEvent.MaxVolume)
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.InvalidValue, 
                $"{SfxEventXmlTags.MinVolume} should not be higher than {SfxEventXmlTags.MaxVolume} for SFXEvent '{sfxEvent.Name}'"));
        }

        if (sfxEvent.MinPitch > sfxEvent.MaxPitch)
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.InvalidValue,
                $"{SfxEventXmlTags.MinPitch} should not be higher than {SfxEventXmlTags.MaxPitch} for SFXEvent '{sfxEvent.Name}'"));
        }

        if (sfxEvent.MinPan2D > sfxEvent.MaxPan2D)
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.InvalidValue,
                $"{SfxEventXmlTags.MinPan2D} should not be higher than {SfxEventXmlTags.MaxPan2D} for SFXEvent '{sfxEvent.Name}'"));
        }

        if (sfxEvent.MinPredelay > sfxEvent.MaxPredelay)
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.InvalidValue,
                $"{SfxEventXmlTags.MinPredelay} should not be higher than {SfxEventXmlTags.MaxPredelay} for SFXEvent '{sfxEvent.Name}'"));
        }

        if (sfxEvent.MinPostdelay > sfxEvent.MaxPostdelay)
        {
            OnParseError(new XmlParseErrorEventArgs(element, XmlParseErrorKind.InvalidValue,
                $"{SfxEventXmlTags.MinPostdelay} should not be higher than {SfxEventXmlTags.MaxPostdelay} for SFXEvent '{sfxEvent.Name}'"));
        }
    }

    protected override bool ParseTag(XElement tag, SfxEvent sfxEvent)
    {
        switch (tag.Name.LocalName)
        {
            case SfxEventXmlTags.OverlapTest:
                sfxEvent.OverlapTestName = PrimitiveParserProvider.StringParser.Parse(tag);
                return true;
            case SfxEventXmlTags.ChainedSfxEvent:
                sfxEvent.ChainedSfxEventName = PrimitiveParserProvider.StringParser.Parse(tag);
                return true;
            case SfxEventXmlTags.UsePreset:
            {
                var presetName = PrimitiveParserProvider.StringParser.Parse(tag);
                var presetNameCrc = HashingService.GetCrc32Upper(presetName.AsSpan(), PGConstants.DefaultPGEncoding);
                if (presetNameCrc != default && ParsedElements.TryGetFirstValue(presetNameCrc, out var preset))
                    sfxEvent.ApplyPreset(preset);
                else
                {
                    OnParseError(new XmlParseErrorEventArgs(tag, 
                        XmlParseErrorKind.MissingReference, $"Cannot to find preset '{presetName}' for SFXEvent '{sfxEvent.Name}'"));
                }
                return true;
            }

            case SfxEventXmlTags.IsPreset:
                sfxEvent.IsPreset = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case SfxEventXmlTags.Is3D:
                sfxEvent.Is3D = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case SfxEventXmlTags.Is2D:
                sfxEvent.Is2D = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case SfxEventXmlTags.IsGui:
                sfxEvent.IsGui = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case SfxEventXmlTags.IsHudVo:
                sfxEvent.IsHudVo = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case SfxEventXmlTags.IsUnitResponseVo:
                sfxEvent.IsUnitResponseVo = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case SfxEventXmlTags.IsAmbientVo:
                sfxEvent.IsAmbientVo = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case SfxEventXmlTags.Localize:
                sfxEvent.IsLocalized = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case SfxEventXmlTags.PlaySequentially:
                sfxEvent.PlaySequentially = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case SfxEventXmlTags.KillsPreviousObjectSFX:
                sfxEvent.KillsPreviousObjectsSfx = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;

            case SfxEventXmlTags.Samples:
                sfxEvent.Samples = new ReadOnlyCollection<string>(PrimitiveParserProvider.LooseStringListParser.Parse(tag));
                return true;
            case SfxEventXmlTags.PreSamples:
                sfxEvent.PreSamples = new ReadOnlyCollection<string>(PrimitiveParserProvider.LooseStringListParser.Parse(tag));
                return true;
            case SfxEventXmlTags.PostSamples:
                sfxEvent.PostSamples = new ReadOnlyCollection<string>(PrimitiveParserProvider.LooseStringListParser.Parse(tag));
                return true;
            case SfxEventXmlTags.TextID:
                sfxEvent.LocalizedTextIDs = new ReadOnlyCollection<string>(PrimitiveParserProvider.LooseStringListParser.Parse(tag));
                return true;

            case SfxEventXmlTags.Priority:
                sfxEvent.Priority = (byte)PrimitiveParserProvider.IntParser.ParseWithRange(tag, SfxEvent.MinPriorityValue, SfxEvent.MaxPriorityValue);
                return true;
            case SfxEventXmlTags.MinPitch:
                sfxEvent.MinPitch = (byte)PrimitiveParserProvider.IntParser.ParseWithRange(tag, SfxEvent.MinPitchValue, SfxEvent.MaxPitchValue);
                return true;
            case SfxEventXmlTags.MaxPitch:
                sfxEvent.MaxPitch = (byte)PrimitiveParserProvider.IntParser.ParseWithRange(tag, SfxEvent.MinPitchValue, SfxEvent.MaxPitchValue);
                return true;
            case SfxEventXmlTags.MinPan2D:
                sfxEvent.MinPan2D = (byte)PrimitiveParserProvider.IntParser.ParseWithRange(tag, byte.MinValue, SfxEvent.MaxPan2dValue);
                return true;
            case SfxEventXmlTags.MaxPan2D:
                sfxEvent.MaxPan2D = (byte)PrimitiveParserProvider.IntParser.ParseWithRange(tag, byte.MinValue, SfxEvent.MaxPan2dValue);
                return true;
            case SfxEventXmlTags.PlayCount:
                sfxEvent.PlayCount = (sbyte)PrimitiveParserProvider.IntParser.ParseWithRange(tag, SfxEvent.InfinitivePlayCount, sbyte.MaxValue);
                return true;
            case SfxEventXmlTags.MaxInstances:
                sfxEvent.MaxInstances = (sbyte)PrimitiveParserProvider.IntParser.ParseWithRange(tag, SfxEvent.MinMaxInstances, sbyte.MaxValue);
                return true;

            case SfxEventXmlTags.Probability:
                sfxEvent.Probability = PrimitiveParserProvider.Max100ByteParser.ParseWithRange(tag, byte.MinValue, SfxEvent.MaxProbability);
                return true;
            case SfxEventXmlTags.MinVolume:
                sfxEvent.MinVolume = PrimitiveParserProvider.Max100ByteParser.ParseWithRange(tag, byte.MinValue, SfxEvent.MaxVolumeValue);
                return true;
            case SfxEventXmlTags.MaxVolume:
                sfxEvent.MaxVolume = PrimitiveParserProvider.Max100ByteParser.ParseWithRange(tag, byte.MinValue, SfxEvent.MaxVolumeValue);
                return true;

            case SfxEventXmlTags.MinPredelay:
                sfxEvent.MinPredelay = PrimitiveParserProvider.UIntParser.Parse(tag);
                return true;
            case SfxEventXmlTags.MaxPredelay:
                sfxEvent.MaxPredelay = PrimitiveParserProvider.UIntParser.Parse(tag);
                return true;
            case SfxEventXmlTags.MinPostdelay:
                sfxEvent.MinPostdelay = PrimitiveParserProvider.UIntParser.Parse(tag);
                return true;
            case SfxEventXmlTags.MaxPostdelay:
                sfxEvent.MaxPostdelay = PrimitiveParserProvider.UIntParser.Parse(tag);
                return true;

            case SfxEventXmlTags.LoopFadeInSeconds:
                sfxEvent.LoopFadeInSeconds = PrimitiveParserProvider.FloatParser.ParseAtLeast(tag, SfxEvent.MinLoopSeconds);
                return true;
            case SfxEventXmlTags.LoopFadeOutSeconds:
                sfxEvent.LoopFadeOutSeconds = PrimitiveParserProvider.FloatParser.ParseAtLeast(tag, SfxEvent.MinLoopSeconds);
                return true;
            case SfxEventXmlTags.VolumeSaturationDistance:
                // I think it was planned at some time to support -1.0 and >= 0.0, since you don't get a warning when -1.0 is coded
                // but the Engine coerces anything < 0.0 to 0.0.
                sfxEvent.VolumeSaturationDistance = PrimitiveParserProvider.FloatParser.ParseAtLeast(tag, SfxEvent.MinVolumeSaturation);
                return true;
            default: return false;
        }
    }

    public override SfxEvent Parse(XElement element) => throw new NotSupportedException();
}