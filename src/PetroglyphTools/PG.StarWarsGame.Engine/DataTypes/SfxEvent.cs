using System;
using System.Collections.Generic;
using System.Linq;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.Xml.Parsers.Data;
using PG.StarWarsGame.Files.XML;

namespace PG.StarWarsGame.Engine.DataTypes;

public sealed class SfxEvent : XmlObject
{
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

    public bool IsPreset { get; }

    public bool Is3D => LazyInitValue(ref _is3D, SfxEventXmlTags.Is3D, false)!.Value;

    public bool Is2D => LazyInitValue(ref _is2D, SfxEventXmlTags.Is2D, false)!.Value;
    
    public bool IsGui => LazyInitValue(ref _isGui, SfxEventXmlTags.IsGui, false)!.Value;

    public bool IsHudVo => LazyInitValue(ref _isHudVo, SfxEventXmlTags.IsHudVo, false)!.Value;

    public bool IsUnitResponseVo => LazyInitValue(ref _isUnitResponseVo, SfxEventXmlTags.IsUnitResponseVo, false)!.Value;

    public bool IsAmbientVo => LazyInitValue(ref _isAmbientVo, SfxEventXmlTags.IsAmbientVo, false)!.Value;

    public bool IsLocalized => LazyInitValue(ref _isLocalized, SfxEventXmlTags.Localize, false)!.Value;

    public string? UsePresetName { get; }

    public bool PlaySequentially => LazyInitValue(ref _is3D, SfxEventXmlTags.Is3D, false)!.Value;

    public IEnumerable<string> AllSamples => PreSamples.Concat(Samples).Concat(PostSamples);

    public IReadOnlyList<string> PreSamples => LazyInitValue(ref _preSamples, SfxEventXmlTags.PreSamples, Array.Empty<string>());

    public IReadOnlyList<string> Samples => LazyInitValue(ref _samples, SfxEventXmlTags.Samples, Array.Empty<string>());

    public IReadOnlyList<string> PostSamples => LazyInitValue(ref _postSamples, SfxEventXmlTags.PostSamples, Array.Empty<string>());

    public IReadOnlyList<string> LocalizedTextIDs { get; }

    public byte Priority { get; }

    public byte Probability { get; }

    public sbyte PlayCount { get; }

    public float LoopFadeInSeconds { get; }

    public float LoopFadeOutInSeconds { get; }

    public sbyte MaxInstances { get; }

    public byte MinVolume { get; }

    public byte MaxVolume { get; }

    public byte MinPitch { get; }

    public byte MaxPitch { get; }

    public byte MinPan2D { get; }

    public byte MaxPan2D { get; }

    public uint MinPredelay { get; }

    public uint MaxPredelay { get; }

    public uint MinPostdelay { get; }

    public uint MaxPostdelay { get; }

    public float VolumeSaturationDistance { get; }

    public bool KillsPreviousObjectsSfx { get; }

    public string? OverlapTestName { get; }

    public string? ChainedSfxEventName { get; }

    internal SfxEvent(string name, Crc32 nameCrc, IReadOnlyValueListDictionary<string, object?> properties,
        XmlLocationInfo location)
        : base(name, nameCrc, properties, location)
    {

    }
}