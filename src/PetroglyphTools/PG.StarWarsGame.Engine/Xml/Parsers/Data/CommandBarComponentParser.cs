using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using PG.Commons.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.CommandBar.Xml;
using PG.StarWarsGame.Engine.Xml.Tags;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers.Primitives;

namespace PG.StarWarsGame.Engine.Xml.Parsers.Data;

public sealed class CommandBarComponentParser(
    IReadOnlyValueListDictionary<Crc32, CommandBarComponentData> parsedElements,
    IServiceProvider serviceProvider,
    IXmlParserErrorReporter? errorReporter = null)
    : XmlObjectParser<CommandBarComponentData>(parsedElements, serviceProvider, errorReporter)
{
    public override CommandBarComponentData Parse(XElement element, out Crc32 upperNameCrc)
    {
        var name = GetXmlObjectName(element, out upperNameCrc);
        var component = new CommandBarComponentData(name, upperNameCrc, XmlLocationInfo.FromElement(element));
        Parse(component, element, default);
        component.CoerceValues();
        return component;
    }
    
    protected override bool ParseTag(XElement tag, CommandBarComponentData componentData)
    {
        switch (tag.Name.LocalName)
        {
            case CommandBarComponentTags.SelectedTextureName:
                componentData.SelectedTextureNames = new ReadOnlyCollection<string>(PetroglyphXmlLooseStringListParser.Instance.Parse(tag));
                return true;
            case CommandBarComponentTags.BlankTextureName:
                componentData.BlankTextureNames = new ReadOnlyCollection<string>(PetroglyphXmlLooseStringListParser.Instance.Parse(tag));
                return true;
            case CommandBarComponentTags.IconAlternateTextureName:
                componentData.IconAlternateTextureNames = new ReadOnlyCollection<string>(PetroglyphXmlLooseStringListParser.Instance.Parse(tag));
                return true;
            case CommandBarComponentTags.MouseOverTextureName:
                componentData.MouseOverTextureNames = new ReadOnlyCollection<string>(PetroglyphXmlLooseStringListParser.Instance.Parse(tag));
                return true;
            case CommandBarComponentTags.BarTextureName:
                componentData.BarTextureNames = new ReadOnlyCollection<string>(PetroglyphXmlLooseStringListParser.Instance.Parse(tag));
                return true;
            case CommandBarComponentTags.BarOverlayName:
                componentData.BarOverlayNames = new ReadOnlyCollection<string>(PetroglyphXmlLooseStringListParser.Instance.Parse(tag));
                return true;
            case CommandBarComponentTags.AlternateFontName:
                componentData.AlternateFontNames = new ReadOnlyCollection<string>(PetroglyphXmlLooseStringListParser.Instance.Parse(tag));
                return true;
            case CommandBarComponentTags.TooltipText:
                componentData.TooltipTexts = new ReadOnlyCollection<string>(PetroglyphXmlLooseStringListParser.Instance.Parse(tag));
                return true;
            case CommandBarComponentTags.LowerEffectTextureName:
                componentData.LowerEffectTextureNames = new ReadOnlyCollection<string>(PetroglyphXmlLooseStringListParser.Instance.Parse(tag));
                return true;
            case CommandBarComponentTags.UpperEffectTextureName:
                componentData.UpperEffectTextureNames = new ReadOnlyCollection<string>(PetroglyphXmlLooseStringListParser.Instance.Parse(tag));
                return true;
            case CommandBarComponentTags.OverlayTextureName:
                componentData.OverlayTextureNames = new ReadOnlyCollection<string>(PetroglyphXmlLooseStringListParser.Instance.Parse(tag));
                return true;
            case CommandBarComponentTags.Overlay2TextureName:
                componentData.Overlay2TextureNames = new ReadOnlyCollection<string>(PetroglyphXmlLooseStringListParser.Instance.Parse(tag));
                return true;

            case CommandBarComponentTags.IconTextureName:
                componentData.IconTextureName = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.DisabledTextureName:
                componentData.DisabledTextureName = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.FlashTextureName:
                componentData.FlashTextureName = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.BuildTextureName:
                componentData.BuildTextureName = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.ModelName:
                componentData.ModelName = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.BoneName:
                componentData.BoneName = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.CursorTextureName:
                componentData.CursorTextureName = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.FontName:
                componentData.FontName = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.ClickSfx:
                componentData.ClickSfx = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.MouseOverSfx:
                componentData.MouseOverSfx = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.RightClickSfx:
                componentData.RightClickSfx = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.Type:
                componentData.Type = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.Group:
                componentData.Group = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.AssociatedText:
                componentData.AssociatedText = PetroglyphXmlStringParser.Instance.Parse(tag);
                return true;

            case CommandBarComponentTags.DragAndDrop:
                componentData.DragAndDrop = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.DragSelect:
                componentData.DragSelect = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.Receptor:
                componentData.Receptor = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.Toggle:
                componentData.Toggle = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.Tab:
                componentData.Tab = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.Hidden:
                componentData.Hidden = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.ClearColor:
                componentData.ClearColor = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.Disabled:
                componentData.Disabled = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.SwapTexture:
                componentData.SwapTexture = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.DrawAdditive:
                componentData.DrawAdditive = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.Editable:
                componentData.Editable = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.TextOutline:
                componentData.TextOutline = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.Stackable:
                componentData.Stackable = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.ModelOffsetX:
                componentData.ModelOffsetX = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.ModelOffsetY:
                componentData.ModelOffsetY = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.ScaleModelX:
                componentData.ScaleModelX = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.ScaleModelY:
                componentData.ScaleModelY = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.Collideable:
                componentData.Collideable = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.TextEmboss:
                componentData.TextEmboss = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.ShouldGhost:
                componentData.ShouldGhost = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.GhostBaseOnly:
                componentData.GhostBaseOnly = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.CrossFade:
                componentData.CrossFade = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.LeftJustified:
                componentData.LeftJustified = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.RightJustified:
                componentData.RightJustified = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.NoShell:
                componentData.NoShell = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.SnapDrag:
                componentData.SnapDrag = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.SnapLocation:
                componentData.SnapLocation = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.OffsetRender:
                componentData.OffsetRender = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.BlinkFade:
                componentData.BlinkFade = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.NoHiddenCollision:
                componentData.NoHiddenCollision = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.ManualOffset:
                componentData.ManualOffset = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.SelectedAlpha:
                componentData.SelectedAlpha = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.PixelAlign:
                componentData.PixelAlign = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.CanDragStack:
                componentData.CanDragStack = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.CanAnimate:
                componentData.CanAnimate = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.LoopAnim:
                componentData.LoopAnim = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.SmoothBar:
                componentData.SmoothBar = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.OutlinedBar:
                componentData.OutlinedBar = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.DragBack:
                componentData.DragBack = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.LowerEffectAdditive:
                componentData.LowerEffectAdditive = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.UpperEffectAdditive:
                componentData.UpperEffectAdditive = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.ClickShift:
                componentData.ClickShift = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.TutorialScene:
                componentData.TutorialScene = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.DialogScene:
                componentData.DialogScene = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.ShouldRenderAtDragPos:
                componentData.ShouldRenderAtDragPos = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.DisableDarken:
                componentData.DisableDarken = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.AnimateBack:
                componentData.AnimateBack = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.AnimateUpperEffect:
                componentData.AnimateUpperEffect = PetroglyphXmlBooleanParser.Instance.Parse(tag);
                return true;

            case CommandBarComponentTags.Size:
                componentData.Size = PetroglyphXmlVector2FParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.TextOffset:
                componentData.TextOffset = PetroglyphXmlVector2FParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.TextOffset2:
                componentData.TextOffset2 = PetroglyphXmlVector2FParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.Offset:
                componentData.Offset = PetroglyphXmlVector2FParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.DefaultOffset:
                componentData.DefaultOffset = PetroglyphXmlVector2FParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.DefaultOffsetWidescreen:
                componentData.DefaultOffsetWidescreen = PetroglyphXmlVector2FParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.IconOffset:
                componentData.IconOffset = PetroglyphXmlVector2FParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.MouseOverOffset:
                componentData.MouseOverOffset = PetroglyphXmlVector2FParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.DisabledOffset:
                componentData.DisabledOffset = PetroglyphXmlVector2FParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.BuildDialOffset:
                componentData.BuildDialOffset = PetroglyphXmlVector2FParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.BuildDial2Offset:
                componentData.BuildDial2Offset = PetroglyphXmlVector2FParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.LowerEffectOffset:
                componentData.LowerEffectOffset = PetroglyphXmlVector2FParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.UpperEffectOffset:
                componentData.UpperEffectOffset = PetroglyphXmlVector2FParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.OverlayOffset:
                componentData.OverlayOffset = PetroglyphXmlVector2FParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.Overlay2Offset:
                componentData.Overlay2Offset = PetroglyphXmlVector2FParser.Instance.Parse(tag);
                return true;

            case CommandBarComponentTags.MaxTextLength:
                componentData.MaxTextLength = PetroglyphXmlUnsignedIntegerParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.FontPointSize:
                componentData.FontPointSize = (int)PetroglyphXmlUnsignedIntegerParser.Instance.Parse(tag);
                return true;

            case CommandBarComponentTags.Scale:
                componentData.Scale = PetroglyphXmlFloatParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.BlinkRate:
                componentData.BlinkRate = PetroglyphXmlFloatParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.MaxTextWidth:
                componentData.MaxTextWidth = PetroglyphXmlFloatParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.BlinkDuration:
                componentData.BlinkDuration = PetroglyphXmlFloatParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.ScaleDuration:
                componentData.ScaleDuration = PetroglyphXmlFloatParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.AnimFps:
                componentData.AnimFps = PetroglyphXmlFloatParser.Instance.Parse(tag);
                return true;

            case CommandBarComponentTags.BaseLayer:
                componentData.BaseLayer = (int)PetroglyphXmlUnsignedIntegerParser.Instance.Parse(tag);
                return true;
            case CommandBarComponentTags.MaxBarLevel:
                componentData.MaxBarLevel = (int)PetroglyphXmlUnsignedIntegerParser.Instance.Parse(tag);
                return true;


            default: return true;
        }
    }

    public override CommandBarComponentData Parse(XElement element) => throw new NotSupportedException();
}