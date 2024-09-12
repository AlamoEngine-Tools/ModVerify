using System.Collections.Generic;
using System.Linq;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.Xml;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.DataTypes;

public sealed class SfxEvent : NamedXmlObject
{
    private byte _minVolume = DefaultMinVolume;
    private byte _maxVolume = DefaultMaxVolume;
    private byte _minPitch = DefaultMinPitch;
    private byte _maxPitch = DefaultMaxPitch;
    private byte _minPan2D = DefaultMinPan2d;
    private byte _maxPan2D = DefaultMaxPan2d;
    private uint _minPredelay;
    private uint _maxPredelay;
    private uint _minPostdelay;
    private uint _maxPostdelay;

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
    
    public bool IsPreset { get; internal set; }

    public bool Is3D { get; internal set; } = DefaultIs3d;
    
    public bool Is2D { get; internal set; }

    public bool IsGui { get; internal set; }

    public bool IsHudVo { get; internal set; }

    public bool IsUnitResponseVo { get; internal set; }

    public bool IsAmbientVo { get; internal set; }

    public bool IsLocalized { get; internal set; }

    public SfxEvent? Preset { get; internal set; }

    public string? UsePresetName { get; internal set; }

    public bool PlaySequentially { get; internal set; }

    public IEnumerable<string> AllSamples => PreSamples.Concat(Samples).Concat(PostSamples);

    public IReadOnlyList<string> PreSamples { get; internal set; } = [];

    public IReadOnlyList<string> Samples { get; internal set; } = [];

    public IReadOnlyList<string> PostSamples { get; internal set; } = [];

    public IReadOnlyList<string> LocalizedTextIDs { get; internal set; } = [];

    public byte Priority { get; internal set; } = DefaultPriority;

    public byte Probability { get; internal set; } = DefaultProbability;

    public sbyte PlayCount { get; internal set; } = DefaultPlayCount;

    public float LoopFadeInSeconds { get; internal set; }

    public float LoopFadeOutSeconds { get; internal set; }

    public sbyte MaxInstances { get; internal set; } = DefaultMaxInstances;

    public float VolumeSaturationDistance { get; internal set; } = DefaultVolumeSaturationDistance;

    public bool KillsPreviousObjectsSfx { get; internal set; }

    public string? OverlapTestName { get; internal set; }

    public string? ChainedSfxEventName { get; internal set; }

    public byte MinVolume
    {
        get => _minVolume;
        internal set => _minVolume = value;
    }

    public byte MaxVolume
    {
        get => _maxVolume;
        internal set => _maxVolume = value;
    }

    public byte MinPitch
    {
        get => _minPitch;
        internal set => _minPitch = value;
    }

    public byte MaxPitch
    {
        get => _maxPitch;
        internal set => _maxPitch = value;
    }

    public byte MinPan2D
    {
        get => _minPan2D;
        internal set => _minPan2D = value;
    }

    public byte MaxPan2D
    {
        get => _maxPan2D;
        internal set => _maxPan2D = value;
    }

    public uint MinPredelay
    {
        get => _minPredelay;
        internal set => _minPredelay = value;
    }

    public uint MaxPredelay
    {
        get => _maxPredelay;
        internal set => _maxPredelay = value;
    }

    public uint MinPostdelay
    {
        get => _minPostdelay;
        internal set => _minPostdelay = value;
    }

    public uint MaxPostdelay
    {
        get => _maxPostdelay;
        internal set => _maxPostdelay = value;
    }


    internal SfxEvent(string name, Crc32 nameCrc, XmlLocationInfo location)
        : base(name, nameCrc, location)
    {
    }

    internal override void CoerceValues()
    {
        AdjustMinMaxValues(ref _minVolume, ref _maxVolume);
        AdjustMinMaxValues(ref _minPitch, ref _maxPitch);
        AdjustMinMaxValues(ref _minPan2D, ref _maxPan2D);
        AdjustMinMaxValues(ref _minPredelay, ref _maxPredelay);
        AdjustMinMaxValues(ref _minPostdelay, ref _maxPostdelay);
    }

    /*
     * The engine also copies the <Use_Preset> of the preset (which is usually null).
     * As this would cause this SFXEvent loose the information of the coded preset, we do not copy the preset's preset value.
     * Example:
     *
     *  <SfxEvent Name="A">                         <SfxEvent Name="Preset">
     *      <Use_Preset>Preset</UsePreset>              <Is_Preset>Yes</Is_Preset>
     *  </SfxEvent>                                     <Min_Volume>90</Min_Volume>
     *                                              </SfxEvent>
     *
     * Engine Behavior:                 SFXEvent instance(Name: A, Use_Preset: null, Min_Volume: 90)
     * PG.StarWarsGame.Engine Behavior: SFXEvent instance(Name: A, Use_Preset: Preset, Min_Volume: 90)
     */
    public void ApplyPreset(SfxEvent preset)
    {
        Is3D = preset.Is3D;
        Is2D = preset.Is2D;
        IsGui = preset.IsGui;
        IsHudVo = preset.IsHudVo;
        IsUnitResponseVo = preset.IsUnitResponseVo;
        IsAmbientVo = preset.IsAmbientVo;
        IsLocalized = preset.IsLocalized;
        Preset = preset;
        UsePresetName = preset.Name;
        PlaySequentially = preset.PlaySequentially;
        PreSamples = preset.PreSamples;
        Samples = preset.Samples;
        PostSamples = preset.PostSamples;
        LocalizedTextIDs = preset.LocalizedTextIDs;
        Priority = preset.Priority;
        Probability = preset.Probability;
        PlayCount = preset.PlayCount;
        LoopFadeInSeconds = preset.LoopFadeInSeconds;
        LoopFadeOutSeconds = preset.LoopFadeOutSeconds;
        MaxInstances = preset.MaxInstances;
        MinPredelay = preset.MinPredelay;
        MaxPredelay = preset.MaxPredelay;
        MinPostdelay = preset.MinPostdelay;
        MaxPostdelay = preset.MaxPostdelay;
        OverlapTestName = preset.OverlapTestName;
        ChainedSfxEventName = preset.ChainedSfxEventName;
        MinVolume = preset.MinVolume;
        MaxVolume = preset.MaxVolume;
        MinPitch = preset.MinPitch;
        MaxPitch = preset.MaxPitch;
        MinPan2D = preset.MinPan2D;
        MaxPan2D = preset.MaxPan2D;
    }
    
    private static void AdjustMinMaxValues(ref byte minValue, ref byte maxValue)
    {
        if (minValue > maxValue)
            minValue = maxValue;
        if (maxValue < minValue)
            maxValue = minValue;
    }

    private static void AdjustMinMaxValues(ref uint minValue, ref uint maxValue)
    {
        if (minValue > maxValue)
            minValue = maxValue;
        if (maxValue < minValue)
            maxValue = minValue;
    }
}