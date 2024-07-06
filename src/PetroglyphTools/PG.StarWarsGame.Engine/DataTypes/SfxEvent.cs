using System;
using System.Collections.Generic;
using System.Linq;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.Xml.Tags;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.DataTypes;

public sealed class SfxEvent : XmlObject
{
    public const byte MaxVolumeValue = 100;
    public const byte MaxPitchValue = 200;
    public const byte MinPitchValue = 50;
    public const byte MaxPan2dValue = 100;
    public const byte MinPriorityValue = 1;
    public const byte MaxPriorityValue = 5;
    public const byte MaxProbability = 100;
    public const sbyte MinMaxInstances = 0;
    public const sbyte InfinitivePlayCount = -1;
    public const float MinLoopSeconds = 0.0f;
    public const float MinVolumeSaturation = 0.0f;

    // Default values which are not the default value of the type
    public const byte DefaultPriority = 3;
    public const bool DefaultIs3d = true;
    public const byte DefaultProbability = 100;
    public const sbyte DefaultPlayCount = 1;
    public const sbyte DefaultMaxInstances = 1;
    public const byte DefaultMinVolume = 100;
    public const byte DefaultMaxVolume = 100;
    public const byte DefaultMinPitch = 100;
    public const byte DefaultMaxPitch = 100;
    public const byte DefaultMinPan2d = 50;
    public const byte DefaultMaxPan2d = 50;
    public const float DefaultVolumeSaturationDistance = 300.0f;

    private SfxEvent? _preset;
    private string? _presetName;
    private string? _overlapTestName;
    private string? _chainedSfxEvent;
    private IReadOnlyList<string>? _textIds;
    private IReadOnlyList<string>? _preSamples;
    private IReadOnlyList<string>? _samples;
    private IReadOnlyList<string>? _postSamples;
    private bool? _isPreset;
    private bool? _is2D;
    private bool? _is3D;
    private bool? _isGui;
    private bool? _isHudVo;
    private bool? _isUnitResponseVo;
    private bool? _isAmbientVo;
    private bool? _isLocalized;
    private bool? _playSequentially;
    private bool? _killsPreviousObjectsSfx;
    private byte? _priority;
    private byte? _probability;
    private sbyte? _playCount;
    private sbyte? _maxInstances;
    private uint? _minPredelay;
    private uint? _maxPredelay;
    private uint? _minPostdelay;
    private uint? _maxPostdelay;
    private float? _loopFadeInSeconds;
    private float? _loopFadeOutSeconds;
    private float? _volumeSaturationDistance;

    private static readonly Func<byte?, byte?> PriorityCoercion = priority =>
    {
        if (!priority.HasValue)
            return DefaultPriority;
        if (priority < MinPriorityValue)
            return MinPriorityValue;
        if (priority > MaxPriorityValue)
            return MaxPriorityValue;
        return priority;
    };

    private static readonly Func<sbyte?, sbyte?> MaxInstancesCoercion = maxInstances =>
    {
        if (!maxInstances.HasValue)
            return DefaultMaxInstances;
        if (maxInstances < MinMaxInstances)
            return MinMaxInstances;
        return maxInstances;
    };

    private static readonly Func<byte?, byte?> ProbabilityCoercion = probability =>
    {
        if (!probability.HasValue)
            return DefaultProbability;
        if (probability > MaxProbability)
            return MaxProbability;
        return probability;
    };

    private static readonly Func<sbyte?, sbyte?> PlayCountCoercion = playCount =>
    {
        if (!playCount.HasValue)
            return DefaultPlayCount;
        if (playCount < InfinitivePlayCount)
            return InfinitivePlayCount;
        return playCount;
    };

    private static readonly Func<float?, float?> LoopAndSaturationCoercion = loopSeconds =>
    {
        if (!loopSeconds.HasValue)
            return DefaultVolumeSaturationDistance;
        if (loopSeconds < MinLoopSeconds)
            return MinLoopSeconds;
        return loopSeconds;
    };

    public bool IsPreset => LazyInitValue(ref _isPreset, SfxEventXmlTags.IsPreset, false)!.Value;

    public bool Is3D => LazyInitValue(ref _is3D, SfxEventXmlTags.Is3D, DefaultIs3d)!.Value;

    public bool Is2D => LazyInitValue(ref _is2D, SfxEventXmlTags.Is2D, false)!.Value;
    
    public bool IsGui => LazyInitValue(ref _isGui, SfxEventXmlTags.IsGui, false)!.Value;

    public bool IsHudVo => LazyInitValue(ref _isHudVo, SfxEventXmlTags.IsHudVo, false)!.Value;

    public bool IsUnitResponseVo => LazyInitValue(ref _isUnitResponseVo, SfxEventXmlTags.IsUnitResponseVo, false)!.Value;

    public bool IsAmbientVo => LazyInitValue(ref _isAmbientVo, SfxEventXmlTags.IsAmbientVo, false)!.Value;

    public bool IsLocalized => LazyInitValue(ref _isLocalized, SfxEventXmlTags.Localize, false)!.Value;

    public SfxEvent? Preset => LazyInitValue(ref _preset, SfxEventXmlTags.PresetXRef, null);

    public string? UsePresetName => LazyInitValue(ref _presetName, SfxEventXmlTags.UsePreset, null);

    public bool PlaySequentially => LazyInitValue(ref _playSequentially, SfxEventXmlTags.PlaySequentially, false)!.Value;

    public IEnumerable<string> AllSamples => PreSamples.Concat(Samples).Concat(PostSamples);

    public IReadOnlyList<string> PreSamples => LazyInitValue(ref _preSamples, SfxEventXmlTags.PreSamples, Array.Empty<string>());

    public IReadOnlyList<string> Samples => LazyInitValue(ref _samples, SfxEventXmlTags.Samples, Array.Empty<string>());

    public IReadOnlyList<string> PostSamples => LazyInitValue(ref _postSamples, SfxEventXmlTags.PostSamples, Array.Empty<string>());

    public IReadOnlyList<string> LocalizedTextIDs => LazyInitValue(ref _textIds, SfxEventXmlTags.TextID, Array.Empty<string>());
    
    public byte Priority => LazyInitValue(ref _priority, SfxEventXmlTags.Priority, DefaultPriority, PriorityCoercion)!.Value;

    public byte Probability => LazyInitValue(ref _probability, SfxEventXmlTags.Probability, DefaultProbability, ProbabilityCoercion)!.Value;

    public sbyte PlayCount => LazyInitValue(ref _playCount, SfxEventXmlTags.PlayCount, DefaultPlayCount, PlayCountCoercion)!.Value;

    public float LoopFadeInSeconds => LazyInitValue(ref _loopFadeInSeconds, SfxEventXmlTags.LoopFadeInSeconds, 0f, LoopAndSaturationCoercion)!.Value;

    public float LoopFadeOutSeconds => LazyInitValue(ref _loopFadeOutSeconds, SfxEventXmlTags.LoopFadeOutSeconds, 0f, LoopAndSaturationCoercion)!.Value;

    public sbyte MaxInstances => LazyInitValue(ref _maxInstances, SfxEventXmlTags.MaxInstances, DefaultMaxInstances, MaxInstancesCoercion)!.Value;

    public uint MinPredelay => LazyInitValue(ref _minPredelay, SfxEventXmlTags.MinPredelay, 0u)!.Value;

    public uint MaxPredelay => LazyInitValue(ref _maxPredelay, SfxEventXmlTags.MaxPredelay, 0u)!.Value;

    public uint MinPostdelay => LazyInitValue(ref _minPostdelay, SfxEventXmlTags.MinPostdelay, 0u)!.Value;

    public uint MaxPostdelay => LazyInitValue(ref _maxPostdelay, SfxEventXmlTags.MaxPostdelay, 0u)!.Value;

    public float VolumeSaturationDistance => LazyInitValue(ref _volumeSaturationDistance,
        SfxEventXmlTags.VolumeSaturationDistance, DefaultVolumeSaturationDistance, LoopAndSaturationCoercion)!.Value;

    public bool KillsPreviousObjectsSfx => LazyInitValue(ref _killsPreviousObjectsSfx, SfxEventXmlTags.KillsPreviousObjectSFX, false)!.Value;

    public string? OverlapTestName => LazyInitValue(ref _overlapTestName, SfxEventXmlTags.OverlapTest, null);

    public string? ChainedSfxEventName => LazyInitValue(ref _chainedSfxEvent, SfxEventXmlTags.ChainedSfxEvent, null);

    public byte MinVolume { get; }

    public byte MaxVolume { get; }

    public byte MinPitch { get; }

    public byte MaxPitch { get; }

    public byte MinPan2D { get; }

    public byte MaxPan2D { get; }

    internal SfxEvent(string name, Crc32 nameCrc, IReadOnlyValueListDictionary<string, object?> properties,
        XmlLocationInfo location)
        : base(name, nameCrc, properties, location)
    {
        var minMaxVolume = GetMinMaxVolume(properties);
        MinVolume = minMaxVolume.min;
        MaxVolume = minMaxVolume.max;

        var minMaxPitch = GetMinMaxPitch(properties);
        MinPitch = minMaxPitch.min;
        MaxPitch = minMaxPitch.max;

        var minMaxPan = GetMinMaxPan2d(properties);
        MinPan2D = minMaxPan.min;
        MaxPan2D = minMaxPan.max;
    }

    private static (byte min, byte max) GetMinMaxVolume(IReadOnlyValueListDictionary<string, object?> properties)
    {
        return GetMinMaxValues(properties, SfxEventXmlTags.MinVolume, SfxEventXmlTags.MaxVolume, DefaultMinVolume,
            DefaultMaxVolume, null, MaxVolumeValue);
    }

    private static (byte min, byte max) GetMinMaxPitch(IReadOnlyValueListDictionary<string, object?> properties)
    {
        return GetMinMaxValues(properties, SfxEventXmlTags.MinPitch, SfxEventXmlTags.MaxPitch, DefaultMinPitch,
            DefaultMaxPitch, MinPitchValue, MaxPitchValue);
    }

    private static (byte min, byte max) GetMinMaxPan2d(IReadOnlyValueListDictionary<string, object?> properties)
    {
        return GetMinMaxValues(properties, SfxEventXmlTags.MinPan2D, SfxEventXmlTags.MaxPan2D, DefaultMinPan2d,
            DefaultMaxPan2d, null, MaxPan2dValue);
    }


    private static (byte min, byte max) GetMinMaxValues(
        IReadOnlyValueListDictionary<string, object?> properties,
        string minTag,
        string maxTag,
        byte defaultMin,
        byte defaultMax,
        byte? totalMinValue,
        byte? totalMaxValue)
    {
        var minValue = !properties.TryGetLastValue(minTag, out var minObj) ? defaultMin : Convert.ToByte(minObj);
        var maxValue = !properties.TryGetLastValue(maxTag, out var maxObj) ? defaultMax : Convert.ToByte(maxObj);

        if (totalMaxValue.HasValue)
        {
            if (minValue > totalMaxValue)
                minValue = totalMaxValue.Value;
            if (maxValue > totalMaxValue)
                maxValue = totalMaxValue.Value;
        }

        if (totalMinValue.HasValue)
        {
            if (minValue < totalMinValue)
                minValue = totalMinValue.Value;
            if (maxValue < totalMinValue)
                maxValue = totalMinValue.Value;
        }

        if (minValue > maxValue)
            minValue = maxValue;

        if (maxValue < minValue)
            maxValue = minValue;

        return (minValue, maxValue);
    }
}