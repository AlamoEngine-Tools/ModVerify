using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml.Linq;
using AnakinRaW.CommonUtilities.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.Audio.Sfx;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal class SfxEventParser(IServiceProvider serviceProvider, IXmlParserErrorReporter? errorReporter = null)
    : NamedXmlObjectParser<SfxEvent>(serviceProvider, new SfxEventXmlTagMapper(serviceProvider), errorReporter)
{
    protected override SfxEvent CreateXmlObject(string name, Crc32 nameCrc, XElement element, XmlLocationInfo location)
    {
        return new SfxEvent(name, nameCrc, location);
    }

    protected override void ValidateAndFixupValues(SfxEvent sfxEvent, XElement element)
    {
        if (sfxEvent.Name.Length > PGConstants.MaxSFXEventName)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.TooLongData,
                Message = $"SFXEvent name '{sfxEvent.Name}' is too long."
            });
        }

        if (sfxEvent.Is2D == sfxEvent.Is3D)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"SFXEvent '{sfxEvent.Name}' has the same value for  is2D and is3D."
            });
        }

        if (sfxEvent.MinVolume > sfxEvent.MaxVolume)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"{SfxEventXmlTags.MinVolume} should not be higher than {SfxEventXmlTags.MaxVolume} for SFXEvent '{sfxEvent.Name}'"
            });
        }

        if (sfxEvent.MinPitch > sfxEvent.MaxPitch)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"{SfxEventXmlTags.MinPitch} should not be higher than {SfxEventXmlTags.MaxPitch} for SFXEvent '{sfxEvent.Name}'"
            });
        }

        if (sfxEvent.MinPan2D > sfxEvent.MaxPan2D)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"{SfxEventXmlTags.MinPan2D} should not be higher than {SfxEventXmlTags.MaxPan2D} for SFXEvent '{sfxEvent.Name}'"
            });
        }

        if (sfxEvent.MinPredelay > sfxEvent.MaxPredelay)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"{SfxEventXmlTags.MinPredelay} should not be higher than {SfxEventXmlTags.MaxPredelay} for SFXEvent '{sfxEvent.Name}'"
            });
        }

        if (sfxEvent.MinPostdelay > sfxEvent.MaxPostdelay)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.InvalidValue,
                Message = $"{SfxEventXmlTags.MinPostdelay} should not be higher than {SfxEventXmlTags.MaxPostdelay} for SFXEvent '{sfxEvent.Name}'"
            });
        }

        sfxEvent.FixupValues();
    }

    protected override bool ParseTag(XElement tag, SfxEvent sfxEvent,
        in IReadOnlyFrugalValueListDictionary<Crc32, SfxEvent> parsedEntries)
    {
        if (tag.Name.LocalName == SfxEventXmlTags.UsePreset)
        {
            var presetName = PetroglyphXmlStringParser.Instance.Parse(tag);

            Debug.Assert(!string.IsNullOrEmpty(presetName));

            var presetNameCrc = HashingService.GetCrc32Upper(presetName.AsSpan(), PGConstants.DefaultPGEncoding);
            if (presetNameCrc != default && parsedEntries.TryGetFirstValue(presetNameCrc, out var preset))
                sfxEvent.ApplyPreset(preset);
            else
            {
                ErrorReporter?.Report(new XmlError(this, tag)
                {
                    Message = $"Cannot to find preset '{presetName}' for SFXEvent '{sfxEvent.Name}'",
                    ErrorKind = XmlParseErrorKind.MissingReference
                });
            }

            // Set the preset value regardless whether we found the SFXEvent or not. 
            sfxEvent.UsePresetName = presetName;

            return true;
        }

        return base.ParseTag(tag, sfxEvent, parsedEntries);
    }


    internal sealed class SfxEventXmlTagMapper(IServiceProvider serviceProvider)
        : XmlTagMapper<SfxEvent>(serviceProvider)
    {
        protected override void BuildMappings()
        {
            AddMapping(
                SfxEventXmlTags.IsPreset,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.IsPreset = val);
            AddMapping(
                SfxEventXmlTags.UsePreset,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.UsePresetName = val);
            AddMapping(
                SfxEventXmlTags.Samples,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val) => obj.Samples = new ReadOnlyCollection<string>(val));
            AddMapping(
                SfxEventXmlTags.PreSamples,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val) => obj.PreSamples = new ReadOnlyCollection<string>(val));
            AddMapping(
                SfxEventXmlTags.PostSamples,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val) => obj.PostSamples = new ReadOnlyCollection<string>(val));
            AddMapping(
                SfxEventXmlTags.TextID,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val) => obj.LocalizedTextIDs = new ReadOnlyCollection<string>(val));
            AddMapping(
                SfxEventXmlTags.PlaySequentially,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.PlaySequentially = val);
            AddMapping(
                SfxEventXmlTags.Priority,
                x => PetroglyphXmlByteParser.Instance.ParseClamped(x, SfxEvent.MinPriorityValue, SfxEvent.MaxPriorityValue),
                (obj, val) => obj.Priority = val);
            AddMapping(
                SfxEventXmlTags.Probability,
                x => PetroglyphXmlBytePercentParser.Instance.ParseAtMost(x, SfxEvent.MaxProbabilityValue),
                (obj, val) => obj.Probability = val);
            AddMapping(
                SfxEventXmlTags.PlayCount,
                x => PetroglyphXmlSByteParser.Instance.ParseAtLeast(x, SfxEvent.InfinitivePlayCountValue),
                (obj, val) => obj.PlayCount = val);
            AddMapping(
                SfxEventXmlTags.LoopFadeInSeconds,
                x => PetroglyphXmlFloatParser.Instance.ParseAtLeast(x, SfxEvent.MinLoopSecondsValue),
                (obj, val) => obj.LoopFadeInSeconds = val);
            AddMapping(
                SfxEventXmlTags.LoopFadeOutSeconds,
                x => PetroglyphXmlFloatParser.Instance.ParseAtLeast(x, SfxEvent.MinLoopSecondsValue),
                (obj, val) => obj.LoopFadeOutSeconds = val);
            AddMapping(
                SfxEventXmlTags.MaxInstances,
                x => PetroglyphXmlSByteParser.Instance.ParseAtLeast(x, SfxEvent.MinMaxInstancesValue),
                (obj, val) => obj.MaxInstances = val);
            AddMapping(
                SfxEventXmlTags.MinVolume,
                x => PetroglyphXmlBytePercentParser.Instance.ParseAtMost(x, SfxEvent.MaxVolumeValue),
                (obj, val) => obj.MinVolume = val);
            AddMapping(
                SfxEventXmlTags.MaxVolume,
                x => PetroglyphXmlBytePercentParser.Instance.ParseAtMost(x, SfxEvent.MaxVolumeValue),
                (obj, val) => obj.MaxVolume = val);
            AddMapping(
                SfxEventXmlTags.MinPitch,
                x => PetroglyphXmlByteParser.Instance.ParseClamped(x, SfxEvent.MinPitchValue, SfxEvent.MaxPitchValue),
                (obj, val) => obj.MinPitch = val);
            AddMapping(
                SfxEventXmlTags.MaxPitch,
                x => PetroglyphXmlByteParser.Instance.ParseClamped(x, SfxEvent.MinPitchValue, SfxEvent.MaxPitchValue),
                (obj, val) => obj.MaxPitch = val);
            AddMapping(
                SfxEventXmlTags.MinPan2D,
                x => PetroglyphXmlByteParser.Instance.ParseAtMost(x, SfxEvent.MaxPan2dValue),
                (obj, val) => obj.MinPan2D = val);
            AddMapping(
                SfxEventXmlTags.MaxPan2D,
                x => PetroglyphXmlByteParser.Instance.ParseAtMost(x, SfxEvent.MaxPan2dValue),
                (obj, val) => obj.MaxPan2D = val);
            AddMapping(
                SfxEventXmlTags.MinPredelay,
                PetroglyphXmlUnsignedIntegerParser.Instance.Parse,
                (obj, val) => obj.MinPredelay = val);
            AddMapping(
                SfxEventXmlTags.MaxPredelay,
                PetroglyphXmlUnsignedIntegerParser.Instance.Parse,
                (obj, val) => obj.MaxPredelay = val);
            AddMapping(
                SfxEventXmlTags.MinPostdelay,
                PetroglyphXmlUnsignedIntegerParser.Instance.Parse,
                (obj, val) => obj.MinPostdelay = val);
            AddMapping(
                SfxEventXmlTags.MaxPostdelay,
                PetroglyphXmlUnsignedIntegerParser.Instance.Parse,
                (obj, val) => obj.MaxPostdelay = val);
            AddMapping(
                SfxEventXmlTags.VolumeSaturationDistance,
                // I think it was planned at some time to support -1.0 and >= 0.0, since you don't get a warning when -1.0 is coded
                // but the Engine coerces anything < 0.0 to 0.0.
                x => PetroglyphXmlFloatParser.Instance.ParseAtLeast(x, SfxEvent.MinVolumeSaturationValue),
                (obj, val) => obj.VolumeSaturationDistance = val);
            AddMapping(
                SfxEventXmlTags.KillsPreviousObjectSFX,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.KillsPreviousObjectsSfx = val);
            AddMapping(
                SfxEventXmlTags.OverlapTest,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.OverlapTestName = val);
            AddMapping(
                SfxEventXmlTags.Localize,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.IsLocalized = val);
            AddMapping(
                SfxEventXmlTags.Is2D,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.Is2D = val);
            AddMapping(
                SfxEventXmlTags.Is3D,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.Is3D = val);
            AddMapping(
                SfxEventXmlTags.IsGui,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.IsGui = val);
            AddMapping(
                SfxEventXmlTags.IsHudVo,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.IsHudVo = val);
            AddMapping(
                SfxEventXmlTags.IsUnitResponseVo,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.IsUnitResponseVo = val);
            AddMapping(
                SfxEventXmlTags.IsAmbientVo,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.IsAmbientVo = val);
            AddMapping(
                SfxEventXmlTags.ChainedSfxEvent,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.ChainedSfxEventName = val);
        }
    }

    internal static class SfxEventXmlTags
    {
        internal const string PresetXRef = "XREF_PRESET";
        internal const string IsPreset = "Is_Preset";
        internal const string UsePreset = "Use_Preset";
        internal const string Samples = "Samples";
        internal const string PreSamples = "Pre_Samples";
        internal const string PostSamples = "Post_Samples";
        internal const string TextID = "Text_ID";
        internal const string PlaySequentially = "Play_Sequentially";
        internal const string Priority = "Priority";
        internal const string Probability = "Probability";
        internal const string PlayCount = "Play_Count";
        internal const string LoopFadeInSeconds = "Loop_Fade_In_Seconds";
        internal const string LoopFadeOutSeconds = "Loop_Fade_Out_Seconds";
        internal const string MaxInstances = "Max_Instances";
        internal const string MinVolume = "Min_Volume";
        internal const string MaxVolume = "Max_Volume";
        internal const string MinPitch = "Min_Pitch";
        internal const string MaxPitch = "Max_Pitch";
        internal const string MinPan2D = "Min_Pan2D";
        internal const string MaxPan2D = "Max_Pan2D";
        internal const string MinPredelay = "Min_Predelay";
        internal const string MaxPredelay = "Max_Predelay";
        internal const string MinPostdelay = "Min_Postdelay";
        internal const string MaxPostdelay = "Max_Postdelay";
        internal const string VolumeSaturationDistance = "Volume_Saturation_Distance";
        internal const string KillsPreviousObjectSFX = "Kills_Previous_Object_SFX";
        internal const string OverlapTest = "Overlap_Test";
        internal const string Localize = "Localize";
        internal const string Is2D = "Is_2D";
        internal const string Is3D = "Is_3D";
        internal const string IsGui = "Is_GUI";
        internal const string IsHudVo = "Is_HUD_VO";
        internal const string IsUnitResponseVo = "Is_Unit_Response_VO";
        internal const string IsAmbientVo = "Is_Ambient_VO";
        internal const string ChainedSfxEvent = "Chained_SFXEvent";
    }
}