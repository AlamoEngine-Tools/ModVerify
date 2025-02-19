using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using PG.Commons.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.Audio.Sfx;
using PG.StarWarsGame.Engine.Xml.Tags;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers.Data;

public sealed class SfxEventParser(
    IReadOnlyValueListDictionary<Crc32, SfxEvent> parsedElements,
    IServiceProvider serviceProvider,
    IXmlParserErrorReporter? errorReporter = null)
    : XmlObjectParser<SfxEvent>(parsedElements, serviceProvider, errorReporter)
{ 
    public override SfxEvent Parse(XElement element, out Crc32 crc32)
    {
        var name = GetXmlObjectName(element, out crc32, true);
        var sfxEvent = new SfxEvent(name, crc32, XmlLocationInfo.FromElement(element));
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
                sfxEvent.OverlapTestName = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case SfxEventXmlTags.ChainedSfxEvent:
                sfxEvent.ChainedSfxEventName = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case SfxEventXmlTags.UsePreset:
            {
                var presetName = PetroglyphXmlStringParser.Instance.Parse(tag);
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
                sfxEvent.IsPreset = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case SfxEventXmlTags.Is3D:
                sfxEvent.Is3D = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case SfxEventXmlTags.Is2D:
                sfxEvent.Is2D = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case SfxEventXmlTags.IsGui:
                sfxEvent.IsGui = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case SfxEventXmlTags.IsHudVo:
                sfxEvent.IsHudVo = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case SfxEventXmlTags.IsUnitResponseVo:
                sfxEvent.IsUnitResponseVo = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case SfxEventXmlTags.IsAmbientVo:
                sfxEvent.IsAmbientVo = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case SfxEventXmlTags.Localize:
                sfxEvent.IsLocalized = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case SfxEventXmlTags.PlaySequentially:
                sfxEvent.PlaySequentially = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case SfxEventXmlTags.KillsPreviousObjectSFX:
                sfxEvent.KillsPreviousObjectsSfx = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;

            case SfxEventXmlTags.Samples:
                sfxEvent.Samples = new ReadOnlyCollection<string>(PetroglyphXmlLooseStringListParser.Instance.Parse(tag));
                return true;
            case SfxEventXmlTags.PreSamples:
                sfxEvent.PreSamples = new ReadOnlyCollection<string>(PetroglyphXmlLooseStringListParser.Instance.Parse(tag));
                return true;
            case SfxEventXmlTags.PostSamples:
                sfxEvent.PostSamples = new ReadOnlyCollection<string>(PetroglyphXmlLooseStringListParser.Instance.Parse(tag));
                return true;
            case SfxEventXmlTags.TextID:
                sfxEvent.LocalizedTextIDs = new ReadOnlyCollection<string>(PetroglyphXmlLooseStringListParser.Instance.Parse(tag));
                return true;

            case SfxEventXmlTags.Priority:
                sfxEvent.Priority = (byte)PetroglyphXmlIntegerParser.Instance.ParseWithRange(tag, SfxEvent.MinPriorityValue, SfxEvent.MaxPriorityValue);
                return true;
            case SfxEventXmlTags.MinPitch:
                sfxEvent.MinPitch = (byte)PetroglyphXmlIntegerParser.Instance.ParseWithRange(tag, SfxEvent.MinPitchValue, SfxEvent.MaxPitchValue);
                return true;
            case SfxEventXmlTags.MaxPitch:
                sfxEvent.MaxPitch = (byte)PetroglyphXmlIntegerParser.Instance.ParseWithRange(tag, SfxEvent.MinPitchValue, SfxEvent.MaxPitchValue);
                return true;
            case SfxEventXmlTags.MinPan2D:
                sfxEvent.MinPan2D = (byte)PetroglyphXmlIntegerParser.Instance.ParseWithRange(tag, byte.MinValue, SfxEvent.MaxPan2dValue);
                return true;
            case SfxEventXmlTags.MaxPan2D:
                sfxEvent.MaxPan2D = (byte)PetroglyphXmlIntegerParser.Instance.ParseWithRange(tag, byte.MinValue, SfxEvent.MaxPan2dValue);
                return true;
            case SfxEventXmlTags.PlayCount:
                sfxEvent.PlayCount = (sbyte)PetroglyphXmlIntegerParser.Instance.ParseWithRange(tag, SfxEvent.InfinitivePlayCount, sbyte.MaxValue);
                return true;
            case SfxEventXmlTags.MaxInstances:
                sfxEvent.MaxInstances = (sbyte)PetroglyphXmlIntegerParser.Instance.ParseWithRange(tag, SfxEvent.MinMaxInstances, sbyte.MaxValue);
                return true;

            case SfxEventXmlTags.Probability:
                sfxEvent.Probability = PetroglyphXmlMax100ByteParser.Instance.ParseWithRange(tag, byte.MinValue, SfxEvent.MaxProbability);
                return true;
            case SfxEventXmlTags.MinVolume:
                sfxEvent.MinVolume = PetroglyphXmlMax100ByteParser.Instance.ParseWithRange(tag, byte.MinValue, SfxEvent.MaxVolumeValue);
                return true;
            case SfxEventXmlTags.MaxVolume:
                sfxEvent.MaxVolume = PetroglyphXmlMax100ByteParser.Instance.ParseWithRange(tag, byte.MinValue, SfxEvent.MaxVolumeValue);
                return true;

            case SfxEventXmlTags.MinPredelay:
                sfxEvent.MinPredelay = PetroglyphXmlUnsignedIntegerParser.Instance.Parse(tag);
                return true;
            case SfxEventXmlTags.MaxPredelay:
                sfxEvent.MaxPredelay = PetroglyphXmlUnsignedIntegerParser.Instance.Parse(tag);
                return true;
            case SfxEventXmlTags.MinPostdelay:
                sfxEvent.MinPostdelay = PetroglyphXmlUnsignedIntegerParser.Instance.Parse(tag);
                return true;
            case SfxEventXmlTags.MaxPostdelay:
                sfxEvent.MaxPostdelay = PetroglyphXmlUnsignedIntegerParser.Instance.Parse(tag);
                return true;

            case SfxEventXmlTags.LoopFadeInSeconds:
                sfxEvent.LoopFadeInSeconds = PetroglyphXmlFloatParser.Instance.ParseAtLeast(tag, SfxEvent.MinLoopSeconds);
                return true;
            case SfxEventXmlTags.LoopFadeOutSeconds:
                sfxEvent.LoopFadeOutSeconds = PetroglyphXmlFloatParser.Instance.ParseAtLeast(tag, SfxEvent.MinLoopSeconds);
                return true;
            case SfxEventXmlTags.VolumeSaturationDistance:
                // I think it was planned at some time to support -1.0 and >= 0.0, since you don't get a warning when -1.0 is coded
                // but the Engine coerces anything < 0.0 to 0.0.
                sfxEvent.VolumeSaturationDistance = PetroglyphXmlFloatParser.Instance.ParseAtLeast(tag, SfxEvent.MinVolumeSaturation);
                return true;
            default: return false;
        }
    }

    public override SfxEvent Parse(XElement element) => throw new NotSupportedException();
}