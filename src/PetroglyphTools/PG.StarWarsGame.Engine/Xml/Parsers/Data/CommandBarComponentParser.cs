using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using PG.Commons.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.CommandBar.Xml;
using PG.StarWarsGame.Engine.Xml.Tags;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;

namespace PG.StarWarsGame.Engine.Xml.Parsers.Data;

public sealed class CommandBarComponentParser(
    IReadOnlyValueListDictionary<Crc32, CommandBarComponentData> parsedElements,
    IServiceProvider serviceProvider,
    IXmlParserErrorListener? listener = null)
    : XmlObjectParser<CommandBarComponentData>(parsedElements, serviceProvider, listener)
{
    public override CommandBarComponentData Parse(XElement element, out Crc32 nameCrc)
    {
        var name = GetXmlObjectName(element, out nameCrc);
        var component = new CommandBarComponentData(name, nameCrc, XmlLocationInfo.FromElement(element));
        Parse(component, element, default);
        component.CoerceValues();
        return component;
    }
    
    protected override bool ParseTag(XElement tag, CommandBarComponentData componentData)
    {
        switch (tag.Name.LocalName)
        {
            case CommandBarComponentTags.SelectedTextureName:
                componentData.SelectedTextureNames = new ReadOnlyCollection<string>(PrimitiveParserProvider.LooseStringListParser.Parse(tag));
                return true;
            case CommandBarComponentTags.BlankTextureName:
                componentData.BlankTextureNames = new ReadOnlyCollection<string>(PrimitiveParserProvider.LooseStringListParser.Parse(tag));
                return true;
            case CommandBarComponentTags.IconAlternateTextureName:
                componentData.IconAlternateTextureNames = new ReadOnlyCollection<string>(PrimitiveParserProvider.LooseStringListParser.Parse(tag));
                return true;
            case CommandBarComponentTags.MouseOverTextureName:
                componentData.MouseOverTextureNames = new ReadOnlyCollection<string>(PrimitiveParserProvider.LooseStringListParser.Parse(tag));
                return true;
            case CommandBarComponentTags.BarTextureName:
                componentData.BarTextureNames = new ReadOnlyCollection<string>(PrimitiveParserProvider.LooseStringListParser.Parse(tag));
                return true;
            case CommandBarComponentTags.BarOverlayName:
                componentData.BarOverlayNames = new ReadOnlyCollection<string>(PrimitiveParserProvider.LooseStringListParser.Parse(tag));
                return true;
            case CommandBarComponentTags.AlternateFontName:
                componentData.AlternateFontNames = new ReadOnlyCollection<string>(PrimitiveParserProvider.LooseStringListParser.Parse(tag));
                return true;
            case CommandBarComponentTags.TooltipText:
                componentData.TooltipTexts = new ReadOnlyCollection<string>(PrimitiveParserProvider.LooseStringListParser.Parse(tag));
                return true;
            case CommandBarComponentTags.LowerEffectTextureName:
                componentData.LowerEffectTextureNames = new ReadOnlyCollection<string>(PrimitiveParserProvider.LooseStringListParser.Parse(tag));
                return true;
            case CommandBarComponentTags.UpperEffectTextureName:
                componentData.UpperEffectTextureNames = new ReadOnlyCollection<string>(PrimitiveParserProvider.LooseStringListParser.Parse(tag));
                return true;
            case CommandBarComponentTags.OverlayTextureName:
                componentData.OverlayTextureNames = new ReadOnlyCollection<string>(PrimitiveParserProvider.LooseStringListParser.Parse(tag));
                return true;
            case CommandBarComponentTags.Overlay2TextureName:
                componentData.Overlay2TextureNames = new ReadOnlyCollection<string>(PrimitiveParserProvider.LooseStringListParser.Parse(tag));
                return true;

            case CommandBarComponentTags.IconTextureName:
                componentData.IconTextureName = PrimitiveParserProvider.StringParser.Parse(tag);
                return true;
            case CommandBarComponentTags.DisabledTextureName:
                componentData.DisabledTextureName = PrimitiveParserProvider.StringParser.Parse(tag);
                return true;
            case CommandBarComponentTags.FlashTextureName:
                componentData.FlashTextureName = PrimitiveParserProvider.StringParser.Parse(tag);
                return true;
            case CommandBarComponentTags.BuildTextureName:
                componentData.BuildTextureName = PrimitiveParserProvider.StringParser.Parse(tag);
                return true;
            case CommandBarComponentTags.ModelName:
                componentData.ModelName = PrimitiveParserProvider.StringParser.Parse(tag);
                return true;
            case CommandBarComponentTags.BoneName:
                componentData.BoneName = PrimitiveParserProvider.StringParser.Parse(tag);
                return true;
            case CommandBarComponentTags.CursorTextureName:
                componentData.CursorTextureName = PrimitiveParserProvider.StringParser.Parse(tag);
                return true;
            case CommandBarComponentTags.FontName:
                componentData.FontName = PrimitiveParserProvider.StringParser.Parse(tag);
                return true;
            case CommandBarComponentTags.ClickSfx:
                componentData.ClickSfx = PrimitiveParserProvider.StringParser.Parse(tag);
                return true;
            case CommandBarComponentTags.MouseOverSfx:
                componentData.MouseOverSfx = PrimitiveParserProvider.StringParser.Parse(tag);
                return true;
            case CommandBarComponentTags.RightClickSfx:
                componentData.RightClickSfx = PrimitiveParserProvider.StringParser.Parse(tag);
                return true;
            case CommandBarComponentTags.Type:
                componentData.Type = PrimitiveParserProvider.StringParser.Parse(tag);
                return true;
            case CommandBarComponentTags.Group:
                componentData.Group = PrimitiveParserProvider.StringParser.Parse(tag);
                return true;
            case CommandBarComponentTags.AssociatedText:
                componentData.AssociatedText = PrimitiveParserProvider.StringParser.Parse(tag);
                return true;

            case CommandBarComponentTags.DragAndDrop:
                componentData.DragAndDrop = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.DragSelect:
                componentData.DragSelect = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.Receptor:
                componentData.Receptor = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.Toggle:
                componentData.Toggle = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.Tab:
                componentData.Tab = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.Hidden:
                componentData.Hidden = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.ClearColor:
                componentData.ClearColor = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.Disabled:
                componentData.Disabled = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.SwapTexture:
                componentData.SwapTexture = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.DrawAdditive:
                componentData.DrawAdditive = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.Editable:
                componentData.Editable = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.TextOutline:
                componentData.TextOutline = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.Stackable:
                componentData.Stackable = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.ModelOffsetX:
                componentData.ModelOffsetX = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.ModelOffsetY:
                componentData.ModelOffsetY = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.ScaleModelX:
                componentData.ScaleModelX = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.ScaleModelY:
                componentData.ScaleModelY = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.Collideable:
                componentData.Collideable = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.TextEmboss:
                componentData.TextEmboss = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.ShouldGhost:
                componentData.ShouldGhost = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.GhostBaseOnly:
                componentData.GhostBaseOnly = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.CrossFade:
                componentData.CrossFade = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.LeftJustified:
                componentData.LeftJustified = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.RightJustified:
                componentData.RightJustified = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.NoShell:
                componentData.NoShell = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.SnapDrag:
                componentData.SnapDrag = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.SnapLocation:
                componentData.SnapLocation = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.OffsetRender:
                componentData.OffsetRender = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.BlinkFade:
                componentData.BlinkFade = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.NoHiddenCollision:
                componentData.NoHiddenCollision = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.ManualOffset:
                componentData.ManualOffset = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.SelectedAlpha:
                componentData.SelectedAlpha = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.PixelAlign:
                componentData.PixelAlign = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.CanDragStack:
                componentData.CanDragStack = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.CanAnimate:
                componentData.CanAnimate = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.LoopAnim:
                componentData.LoopAnim = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.SmoothBar:
                componentData.SmoothBar = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.OutlinedBar:
                componentData.OutlinedBar = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.DragBack:
                componentData.DragBack = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.LowerEffectAdditive:
                componentData.LowerEffectAdditive = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.UpperEffectAdditive:
                componentData.UpperEffectAdditive = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.ClickShift:
                componentData.ClickShift = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.TutorialScene:
                componentData.TutorialScene = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.DialogScene:
                componentData.DialogScene = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.ShouldRenderAtDragPos:
                componentData.ShouldRenderAtDragPos = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.DisableDarken:
                componentData.DisableDarken = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.AnimateBack:
                componentData.AnimateBack = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;
            case CommandBarComponentTags.AnimateUpperEffect:
                componentData.AnimateUpperEffect = PrimitiveParserProvider.BooleanParser.Parse(tag);
                return true;

            case CommandBarComponentTags.Size:
                componentData.Size = PrimitiveParserProvider.Vector2FParser.Parse(tag);
                return true;
            case CommandBarComponentTags.TextOffset:
                componentData.TextOffset = PrimitiveParserProvider.Vector2FParser.Parse(tag);
                return true;
            case CommandBarComponentTags.TextOffset2:
                componentData.TextOffset2 = PrimitiveParserProvider.Vector2FParser.Parse(tag);
                return true;
            case CommandBarComponentTags.Offset:
                componentData.Offset = PrimitiveParserProvider.Vector2FParser.Parse(tag);
                return true;
            case CommandBarComponentTags.DefaultOffset:
                componentData.DefaultOffset = PrimitiveParserProvider.Vector2FParser.Parse(tag);
                return true;
            case CommandBarComponentTags.DefaultOffsetWidescreen:
                componentData.DefaultOffsetWidescreen = PrimitiveParserProvider.Vector2FParser.Parse(tag);
                return true;
            case CommandBarComponentTags.IconOffset:
                componentData.IconOffset = PrimitiveParserProvider.Vector2FParser.Parse(tag);
                return true;
            case CommandBarComponentTags.MouseOverOffset:
                componentData.MouseOverOffset = PrimitiveParserProvider.Vector2FParser.Parse(tag);
                return true;
            case CommandBarComponentTags.DisabledOffset:
                componentData.DisabledOffset = PrimitiveParserProvider.Vector2FParser.Parse(tag);
                return true;
            case CommandBarComponentTags.BuildDialOffset:
                componentData.BuildDialOffset = PrimitiveParserProvider.Vector2FParser.Parse(tag);
                return true;
            case CommandBarComponentTags.BuildDial2Offset:
                componentData.BuildDial2Offset = PrimitiveParserProvider.Vector2FParser.Parse(tag);
                return true;
            case CommandBarComponentTags.LowerEffectOffset:
                componentData.LowerEffectOffset = PrimitiveParserProvider.Vector2FParser.Parse(tag);
                return true;
            case CommandBarComponentTags.UpperEffectOffset:
                componentData.UpperEffectOffset = PrimitiveParserProvider.Vector2FParser.Parse(tag);
                return true;
            case CommandBarComponentTags.OverlayOffset:
                componentData.OverlayOffset = PrimitiveParserProvider.Vector2FParser.Parse(tag);
                return true;
            case CommandBarComponentTags.Overlay2Offset:
                componentData.Overlay2Offset = PrimitiveParserProvider.Vector2FParser.Parse(tag);
                return true;

            case CommandBarComponentTags.MaxTextLength:
                componentData.MaxTextLength = PrimitiveParserProvider.UIntParser.Parse(tag);
                return true;
            case CommandBarComponentTags.FontPointSize:
                componentData.FontPointSize = (int)PrimitiveParserProvider.UIntParser.Parse(tag);
                return true;

            case CommandBarComponentTags.Scale:
                componentData.Scale = PrimitiveParserProvider.FloatParser.Parse(tag);
                return true;
            case CommandBarComponentTags.BlinkRate:
                componentData.BlinkRate = PrimitiveParserProvider.FloatParser.Parse(tag);
                return true;
            case CommandBarComponentTags.MaxTextWidth:
                componentData.MaxTextWidth = PrimitiveParserProvider.FloatParser.Parse(tag);
                return true;
            case CommandBarComponentTags.BlinkDuration:
                componentData.BlinkDuration = PrimitiveParserProvider.FloatParser.Parse(tag);
                return true;
            case CommandBarComponentTags.ScaleDuration:
                componentData.ScaleDuration = PrimitiveParserProvider.FloatParser.Parse(tag);
                return true;
            case CommandBarComponentTags.AnimFps:
                componentData.AnimFps = PrimitiveParserProvider.FloatParser.Parse(tag);
                return true;

            case CommandBarComponentTags.BaseLayer:
                componentData.BaseLayer = (int)PrimitiveParserProvider.UIntParser.Parse(tag);
                return true;
            case CommandBarComponentTags.MaxBarLevel:
                componentData.MaxBarLevel = (int)PrimitiveParserProvider.UIntParser.Parse(tag);
                return true;


            default: return true;
        }
    }

    public override CommandBarComponentData Parse(XElement element) => throw new NotSupportedException();
}