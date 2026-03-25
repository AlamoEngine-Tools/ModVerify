using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using PG.Commons.Hashing;
using PG.Commons.Numerics;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Data;

namespace PG.StarWarsGame.Engine.CommandBar.Xml;

public sealed class CommandBarComponentData : NamedXmlObject
{
    public const float DefaultScale = 1.0f;
    public const float DefaultBlinkRate = 0.2f;
    public const int DefaultBaseLayer = 2;
    public static readonly Vector2 DefaultOffsetWidescreenValue = new(9.9999998e17f, 9.9999998e17f);
    public static readonly Vector4Int WhiteColor = new(255, 255, 255, 255);

    internal readonly List<string> SelectedTextureNamesInternal = [];
    internal readonly List<string> BlankTextureNamesInternal = [];
    internal readonly List<string> IconAlternateTextureNamesInternal = [];
    internal readonly List<string> MouseOverTextureNamesInternal = [];
    internal readonly List<string> BarTextureNamesInternal = [];
    internal readonly List<string> BarOverlayNamesInternal = [];
    internal readonly List<string> AlternateFontNamesInternal = [];
    internal readonly List<string> TooltipTextsInternal = [];
    internal readonly List<string> LowerEffectTextureNamesInternal = [];
    internal readonly List<string> UpperEffectTextureNamesInternal = [];
    internal readonly List<string> OverlayTextureNamesInternal = [];
    internal readonly List<string> Overlay2TextureNamesInternal = [];
    
    public IReadOnlyList<string> SelectedTextureNames { get; }
    public IReadOnlyList<string> BlankTextureNames { get; }
    public IReadOnlyList<string> IconAlternateTextureNames { get; }
    public IReadOnlyList<string> MouseOverTextureNames { get; }
    public IReadOnlyList<string> BarTextureNames { get; }
    public IReadOnlyList<string> BarOverlayNames { get; }
    public IReadOnlyList<string> AlternateFontNames { get; }
    public IReadOnlyList<string> TooltipTexts { get; }
    public IReadOnlyList<string> LowerEffectTextureNames { get; }
    public IReadOnlyList<string> UpperEffectTextureNames { get; }
    public IReadOnlyList<string> OverlayTextureNames { get; }
    public IReadOnlyList<string> Overlay2TextureNames { get; }

    public string? IconTextureName { get; internal set; }
    public string? DisabledTextureName { get; internal set; }
    public string? FlashTextureName { get; internal set; }
    public string? BuildTextureName { get; internal set; }
    public string? ModelName { get; internal set; }
    public string? BoneName { get; internal set; }
    public string? CursorTextureName { get; internal set; }
    public string? FontName { get; internal set; }
    public string? ClickSfx { get; internal set; }
    public string? MouseOverSfx { get; internal set; }
    public string? RightClickSfx { get; internal set; }
    public string? Type { get; internal set; }
    public string? Group { get; internal set; }
    public string? AssociatedText { get; internal set; }

    public bool DragAndDrop { get; internal set; }
    public bool DragSelect { get; internal set; }
    public bool Receptor { get; internal set; }
    public bool Toggle { get; internal set; }
    public bool Tab { get; internal set; }
    public bool Hidden { get; internal set; }
    public bool ClearColor { get; internal set; }
    public bool Disabled { get; internal set; }
    public bool SwapTexture { get; internal set; }
    public bool DrawAdditive { get; internal set; }
    public bool Editable { get; internal set; }
    public bool TextOutline { get; internal set; }
    public bool Stackable { get; internal set; }
    public bool ModelOffsetX { get; internal set; }
    public bool ModelOffsetY { get; internal set; }
    public bool ScaleModelX { get; internal set; }
    public bool ScaleModelY { get; internal set; }
    public bool Collideable { get; internal set; }
    public bool TextEmboss { get; internal set; }
    public bool ShouldGhost { get; internal set; }
    public bool GhostBaseOnly { get; internal set; }
    public bool CrossFade { get; internal set; }
    public bool LeftJustified { get; internal set; }
    public bool RightJustified { get; internal set; }
    public bool NoShell { get; internal set; }
    public bool SnapDrag { get; internal set; }
    public bool SnapLocation { get; internal set; }
    public bool OffsetRender { get; internal set; }
    public bool BlinkFade { get; internal set; }
    public bool NoHiddenCollision { get; internal set; }
    public bool ManualOffset { get; internal set; }
    public bool SelectedAlpha { get; internal set; }
    public bool PixelAlign { get; internal set; }
    public bool CanDragStack { get; internal set; }
    public bool CanAnimate { get; internal set; }
    public bool LoopAnim { get; internal set; }
    public bool SmoothBar { get; internal set; }
    public bool OutlinedBar { get; internal set; }
    public bool DragBack { get; internal set; }
    public bool LowerEffectAdditive { get; internal set; }
    public bool UpperEffectAdditive { get; internal set; }
    public bool ClickShift { get; internal set; }
    public bool TutorialScene { get; internal set; }
    public bool DialogScene { get; internal set; }
    public bool ShouldRenderAtDragPos { get; internal set; }
    public bool DisableDarken { get; internal set; }
    public bool AnimateBack { get; internal set; }
    public bool AnimateUpperEffect { get; internal set; }

    public int BaseLayer { get; internal set; } = DefaultBaseLayer;
    public int MaxBarLevel { get; internal set; }

    public uint MaxTextLength { get; internal set; }
    public int FontPointSize { get; internal set; }

    public float ScaleDuration { get; internal set; }
    public float BlinkDuration { get; internal set; }
    public float MaxTextWidth { get; internal set; }
    public float BlinkRate { get; internal set; } = DefaultBlinkRate;
    public float Scale { get; internal set; } = DefaultScale;
    public float AnimFps { get; internal set; }

    public Vector2 Size { get; internal set; }
    public Vector2 TextOffset { get; internal set; }
    public Vector2 TextOffset2 { get; internal set; }
    public Vector2 Offset { get; internal set; }
    public Vector2 DefaultOffset { get; internal set; }
    public Vector2 DefaultOffsetWidescreen { get; internal set; } = DefaultOffsetWidescreenValue;
    public Vector2 IconOffset { get; internal set; }
    public Vector2 MouseOverOffset { get; internal set; }
    public Vector2 DisabledOffset { get; internal set; }
    public Vector2 BuildDialOffset { get; internal set; }
    public Vector2 BuildDial2Offset { get; internal set; }
    public Vector2 LowerEffectOffset { get; internal set; }
    public Vector2 UpperEffectOffset { get; internal set; }
    public Vector2 OverlayOffset { get; internal set; }
    public Vector2 Overlay2Offset { get; internal set; }

    public Vector4Int? Color { get; internal set; } = WhiteColor;
    public Vector4Int? TextColor { get; internal set; }
    public Vector4Int? TextColor2 { get; internal set; }
    public Vector4Int? MaxBarColor { get; internal set; } = WhiteColor;

    public CommandBarComponentData(string name, Crc32 crc, XmlLocationInfo location) : base(name, crc, location)
    {
        SelectedTextureNames = new ReadOnlyCollection<string>(SelectedTextureNamesInternal);
        BlankTextureNames = new ReadOnlyCollection<string>(BlankTextureNamesInternal);
        IconAlternateTextureNames = new ReadOnlyCollection<string>(IconAlternateTextureNamesInternal);
        MouseOverTextureNames = new ReadOnlyCollection<string>(MouseOverTextureNamesInternal);
        BarTextureNames = new ReadOnlyCollection<string>(BarTextureNamesInternal);
        BarOverlayNames = new ReadOnlyCollection<string>(BarOverlayNamesInternal);
        AlternateFontNames = new ReadOnlyCollection<string>(AlternateFontNamesInternal);
        TooltipTexts = new ReadOnlyCollection<string>(TooltipTextsInternal);
        LowerEffectTextureNames = new ReadOnlyCollection<string>(LowerEffectTextureNamesInternal);
        UpperEffectTextureNames = new ReadOnlyCollection<string>(UpperEffectTextureNamesInternal);
        OverlayTextureNames = new ReadOnlyCollection<string>(OverlayTextureNamesInternal);
        Overlay2TextureNames = new ReadOnlyCollection<string>(Overlay2TextureNamesInternal);
    }

    internal void FixupValues()
    {
        for (var i = 0; i < AlternateFontNamesInternal.Count; i++) 
            AlternateFontNamesInternal[i] = AlternateFontNamesInternal[i].Replace('_', ' ');
    }
}

