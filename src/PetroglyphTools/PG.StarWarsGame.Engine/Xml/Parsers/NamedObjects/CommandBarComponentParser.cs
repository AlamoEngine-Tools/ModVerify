using AnakinRaW.CommonUtilities.Collections;
using PG.StarWarsGame.Engine.CommandBar.Xml;
using PG.StarWarsGame.Engine.Xml.Parsers.Tags;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;
using System;
using System.Xml.Linq;
using Crc32 = PG.Commons.Hashing.Crc32;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal class CommandBarComponentParser(
    GameEngineType engine, 
    IServiceProvider serviceProvider, 
    IXmlParserErrorReporter? errorReporter = null)
    : NamedXmlObjectParser<CommandBarComponentData>(engine, new CommandBarComponentDataXmlTagMapper(serviceProvider), errorReporter, serviceProvider)
{
    protected override bool UpperCaseNameForCrc => true;
    protected override bool UpperCaseNameForObject => false;

    protected override CommandBarComponentData CreateXmlObject(
        string name, 
        Crc32 nameCrc,
        XElement element,
        IReadOnlyFrugalValueListDictionary<Crc32, CommandBarComponentData> parsedEntries,
        XmlLocationInfo location)
    {
        return new CommandBarComponentData(name, nameCrc, location);
    }


    protected override void ValidateAndFixupValues(CommandBarComponentData xmlData, XElement element, in IReadOnlyFrugalValueListDictionary<Crc32, CommandBarComponentData> parsedEntries)
    {
        if (xmlData.Name.Length > PGConstants.MaxCommandBarComponentName)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                Message = $"CommandbarComponent name '{xmlData.Name}' is too long.",
                ErrorKind = XmlParseErrorKind.TooLongData
            });
        }

        xmlData.FixupValues();
    }
    
    private sealed class CommandBarComponentDataXmlTagMapper(IServiceProvider serviceProvider)
        : XmlTagMapper<CommandBarComponentData>(serviceProvider)
    {
        protected override void BuildMappings()
        {
            AddMapping(
                CommandBarComponentTags.SelectedTextureName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val, replace) => 
                    SetOrReplaceList(obj.SelectedTextureNamesInternal, val, replace));
            AddMapping(
                CommandBarComponentTags.BlankTextureName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val, replace) => 
                    SetOrReplaceList(obj.BlankTextureNamesInternal, val, replace));
            AddMapping(
                CommandBarComponentTags.IconTextureName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.IconTextureName = val);
            AddMapping(
                CommandBarComponentTags.IconAlternateTextureName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val, replace) => 
                    SetOrReplaceList(obj.IconAlternateTextureNamesInternal, val, replace));
            AddMapping(
                CommandBarComponentTags.MouseOverTextureName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val, replace) => 
                    SetOrReplaceList(obj.MouseOverTextureNamesInternal, val, replace));
            AddMapping(
                CommandBarComponentTags.DisabledTextureName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.DisabledTextureName = val);
            AddMapping(
                CommandBarComponentTags.FlashTextureName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.FlashTextureName = val);
            AddMapping(
                CommandBarComponentTags.BarTextureName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val, replace) => 
                    SetOrReplaceList(obj.BarTextureNamesInternal, val, replace));
            AddMapping(
                CommandBarComponentTags.BarOverlayName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val, replace) => 
                    SetOrReplaceList(obj.BarOverlayNamesInternal, val, replace));
            AddMapping(
                CommandBarComponentTags.BuildTextureName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.BuildTextureName = val);
            AddMapping(
                CommandBarComponentTags.ModelName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.ModelName = val);
            AddMapping(
                CommandBarComponentTags.BoneName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.BoneName = val);
            AddMapping(
                CommandBarComponentTags.CursorTextureName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.CursorTextureName = val);
            AddMapping(
                CommandBarComponentTags.FontName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.FontName = val);
            AddMapping(
                CommandBarComponentTags.AlternateFontName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val, replace) => 
                    SetOrReplaceList(obj.AlternateFontNamesInternal, val, replace));
            AddMapping(
                CommandBarComponentTags.TooltipText,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val, replace) => 
                    SetOrReplaceList(obj.TooltipTextsInternal, val, replace));
            AddMapping(
                CommandBarComponentTags.ClickSfx,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.ClickSfx = val);
            AddMapping(
                CommandBarComponentTags.MouseOverSfx,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.MouseOverSfx = val);
            AddMapping(
                CommandBarComponentTags.LowerEffectTextureName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val, replace) => 
                    SetOrReplaceList(obj.LowerEffectTextureNamesInternal, val, replace));
            AddMapping(
                CommandBarComponentTags.UpperEffectTextureName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val, replace) => 
                    SetOrReplaceList(obj.UpperEffectTextureNamesInternal, val, replace));
            AddMapping(
                CommandBarComponentTags.OverlayTextureName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val, replace) => 
                    SetOrReplaceList(obj.OverlayTextureNamesInternal, val, replace));
            AddMapping(
                CommandBarComponentTags.Overlay2TextureName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val, replace) => 
                    SetOrReplaceList(obj.Overlay2TextureNamesInternal, val, replace));
            AddMapping(
                CommandBarComponentTags.RightClickSfx,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.RightClickSfx = val);
            AddMapping(
                CommandBarComponentTags.Type,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.Type = val);
            AddMapping(
                CommandBarComponentTags.Group,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.Group = val);
            AddMapping(
                CommandBarComponentTags.DragAndDrop,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.DragAndDrop = val);
            AddMapping(
                CommandBarComponentTags.DragSelect,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.DragSelect = val);
            AddMapping(
                CommandBarComponentTags.Receptor,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.Receptor = val);
            AddMapping(
                CommandBarComponentTags.Toggle,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.Toggle = val);
            AddMapping(
                CommandBarComponentTags.Tab,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.Tab = val);
            AddMapping(
                CommandBarComponentTags.AssociatedText,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.AssociatedText = val);
            AddMapping(
                CommandBarComponentTags.Hidden,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.Hidden = val);
            AddMapping(
                CommandBarComponentTags.Scale,
                PetroglyphXmlFloatParser.Instance.Parse,
                (obj, val) => obj.Scale = val);
            AddMapping(
                CommandBarComponentTags.Color,
                PetroglyphXmlRgbaColorParser.Instance.Parse,
                (obj, val) => obj.Color = val);
            AddMapping(
                CommandBarComponentTags.TextColor,
                PetroglyphXmlRgbaColorParser.Instance.Parse,
                (obj, val) => obj.TextColor = val);
            AddMapping(
                CommandBarComponentTags.TextColor2,
                PetroglyphXmlRgbaColorParser.Instance.Parse,
                (obj, val) => obj.TextColor2 = val);
            AddMapping(
                CommandBarComponentTags.Size,
                PetroglyphXmlVector2FParser.Instance.Parse,
                (obj, val) => obj.Size = val);
            AddMapping(
                CommandBarComponentTags.ClearColor,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.ClearColor = val);
            AddMapping(
                CommandBarComponentTags.Disabled,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.Disabled = val);
            AddMapping(
                CommandBarComponentTags.SwapTexture,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.SwapTexture = val);
            AddMapping(
                CommandBarComponentTags.BaseLayer,
                x => (int)PetroglyphXmlUnsignedIntegerParser.Instance.Parse(x),
                (obj, val) => obj.BaseLayer = val);
            AddMapping(
                CommandBarComponentTags.DrawAdditive,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.DrawAdditive = val);
            AddMapping(
               CommandBarComponentTags.TextOffset,
               PetroglyphXmlVector2FParser.Instance.Parse,
               (obj, val) => obj.TextOffset = val);
            AddMapping(
                CommandBarComponentTags.TextOffset2,
                PetroglyphXmlVector2FParser.Instance.Parse,
                (obj, val) => obj.TextOffset2 = val);
            AddMapping(
                CommandBarComponentTags.Offset,
                PetroglyphXmlVector2FParser.Instance.Parse,
                (obj, val) => obj.Offset = val);
            AddMapping(
                CommandBarComponentTags.DefaultOffset,
                PetroglyphXmlVector2FParser.Instance.Parse,
                (obj, val) => obj.DefaultOffset = val);
            AddMapping(
                CommandBarComponentTags.DefaultOffsetWidescreen,
                PetroglyphXmlVector2FParser.Instance.Parse,
                (obj, val) => obj.DefaultOffsetWidescreen = val);
            AddMapping(
                CommandBarComponentTags.IconOffset,
                PetroglyphXmlVector2FParser.Instance.Parse,
                (obj, val) => obj.IconOffset = val);
            AddMapping(
                CommandBarComponentTags.MouseOverOffset,
                PetroglyphXmlVector2FParser.Instance.Parse,
                (obj, val) => obj.MouseOverOffset = val);
            AddMapping(
                CommandBarComponentTags.DisabledOffset,
                PetroglyphXmlVector2FParser.Instance.Parse,
                (obj, val) => obj.DisabledOffset = val);
            AddMapping(
                CommandBarComponentTags.BuildDialOffset,
                PetroglyphXmlVector2FParser.Instance.Parse,
                (obj, val) => obj.BuildDialOffset = val);
            AddMapping(
                CommandBarComponentTags.BuildDial2Offset,
                PetroglyphXmlVector2FParser.Instance.Parse,
                (obj, val) => obj.BuildDial2Offset = val);
            AddMapping(
                CommandBarComponentTags.LowerEffectOffset,
                PetroglyphXmlVector2FParser.Instance.Parse,
                (obj, val) => obj.LowerEffectOffset = val);
            AddMapping(
                CommandBarComponentTags.UpperEffectOffset,
                PetroglyphXmlVector2FParser.Instance.Parse,
                (obj, val) => obj.UpperEffectOffset = val);
            AddMapping(
                CommandBarComponentTags.OverlayOffset,
                PetroglyphXmlVector2FParser.Instance.Parse,
                (obj, val) => obj.OverlayOffset = val);
            AddMapping(
                CommandBarComponentTags.Overlay2Offset,
                PetroglyphXmlVector2FParser.Instance.Parse,
                (obj, val) => obj.Overlay2Offset = val);
            AddMapping(
                CommandBarComponentTags.Editable,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.Editable = val);
            AddMapping(
                CommandBarComponentTags.MaxTextLength,
                PetroglyphXmlUnsignedIntegerParser.Instance.Parse,
                (obj, val) => obj.MaxTextLength = val);
            AddMapping(
                CommandBarComponentTags.BlinkRate,
                PetroglyphXmlFloatParser.Instance.Parse,
                (obj, val) => obj.BlinkRate = val);
            AddMapping(
                CommandBarComponentTags.FontPointSize,
                x => (int)PetroglyphXmlUnsignedIntegerParser.Instance.Parse(x),
                (obj, val) => obj.FontPointSize = val);
            AddMapping(
                CommandBarComponentTags.TextOutline,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.TextOutline = val);
            AddMapping(
                CommandBarComponentTags.MaxTextWidth,
                PetroglyphXmlFloatParser.Instance.Parse,
                (obj, val) => obj.MaxTextWidth = val);
            AddMapping(
                CommandBarComponentTags.Stackable,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.Stackable = val);
            AddMapping(
                CommandBarComponentTags.ModelOffsetX,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.ModelOffsetX = val);
            AddMapping(
                CommandBarComponentTags.ModelOffsetY,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.ModelOffsetY = val);
            AddMapping(
                CommandBarComponentTags.ScaleModelX,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.ScaleModelX = val);
            AddMapping(
                CommandBarComponentTags.ScaleModelY,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.ScaleModelY = val);
            AddMapping(
                CommandBarComponentTags.Collideable,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.Collideable = val);
            AddMapping(
                CommandBarComponentTags.TextEmboss,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.TextEmboss = val);
            AddMapping(
                CommandBarComponentTags.ShouldGhost,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.ShouldGhost = val);
            AddMapping(
                CommandBarComponentTags.GhostBaseOnly,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.GhostBaseOnly = val);
            AddMapping(
                CommandBarComponentTags.MaxBarLevel,
                x => PetroglyphXmlIntegerParser.Instance.Parse(x),
                (obj, val) => obj.MaxBarLevel = val);
            AddMapping(
                CommandBarComponentTags.MaxBarColor,
                PetroglyphXmlRgbaColorParser.Instance.Parse,
                (obj, val) => obj.MaxBarColor = val);
            AddMapping(
                CommandBarComponentTags.CrossFade,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.CrossFade = val);
            AddMapping(
                CommandBarComponentTags.LeftJustified,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.LeftJustified = val);
            AddMapping(
                CommandBarComponentTags.RightJustified,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.RightJustified = val);
            AddMapping(
                CommandBarComponentTags.NoShell,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.NoShell = val);
            AddMapping(
                CommandBarComponentTags.SnapDrag,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.SnapDrag = val);
            AddMapping(
                CommandBarComponentTags.SnapLocation,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.SnapLocation = val);
            AddMapping(
                CommandBarComponentTags.BlinkDuration,
                PetroglyphXmlFloatParser.Instance.Parse,
                (obj, val) => obj.BlinkDuration = val);
            AddMapping(
                CommandBarComponentTags.ScaleDuration,
                PetroglyphXmlFloatParser.Instance.Parse,
                (obj, val) => obj.ScaleDuration = val);
            AddMapping(
                CommandBarComponentTags.OffsetRender,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.OffsetRender = val);
            AddMapping(
                CommandBarComponentTags.BlinkFade,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.BlinkFade = val);
            AddMapping(
                CommandBarComponentTags.NoHiddenCollision,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.NoHiddenCollision = val);
            AddMapping(
                CommandBarComponentTags.ManualOffset,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.ManualOffset = val);
            AddMapping(
                CommandBarComponentTags.SelectedAlpha,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.SelectedAlpha = val);
            AddMapping(
                CommandBarComponentTags.PixelAlign,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.PixelAlign = val);
            AddMapping(
                CommandBarComponentTags.CanDragStack,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.CanDragStack = val);
            AddMapping(
                CommandBarComponentTags.CanAnimate,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.CanAnimate = val);
            AddMapping(
                CommandBarComponentTags.AnimFps,
                PetroglyphXmlFloatParser.Instance.Parse,
                (obj, val) => obj.AnimFps = val);
            AddMapping(
                CommandBarComponentTags.LoopAnim,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.LoopAnim = val);
            AddMapping(
                CommandBarComponentTags.SmoothBar,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.SmoothBar = val);
            AddMapping(
                CommandBarComponentTags.OutlinedBar,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.OutlinedBar = val);
            AddMapping(
                CommandBarComponentTags.DragBack,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.DragBack = val);
            AddMapping(
                CommandBarComponentTags.LowerEffectAdditive,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.LowerEffectAdditive = val);
            AddMapping(
                CommandBarComponentTags.UpperEffectAdditive,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.UpperEffectAdditive = val);
            AddMapping(
                CommandBarComponentTags.ClickShift,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.ClickShift = val);
            AddMapping(
                CommandBarComponentTags.TutorialScene,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.TutorialScene = val);
            AddMapping(
                CommandBarComponentTags.DialogScene,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.DialogScene = val);
            AddMapping(
                CommandBarComponentTags.ShouldRenderAtDragPos,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.ShouldRenderAtDragPos = val);
            AddMapping(
                CommandBarComponentTags.DisableDarken,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.DisableDarken = val);
            AddMapping(
                CommandBarComponentTags.AnimateBack,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.AnimateBack = val);
            AddMapping(
                CommandBarComponentTags.AnimateUpperEffect,
                PetroglyphXmlBooleanParser.Instance.Parse,
                (obj, val) => obj.AnimateUpperEffect = val);
        }
    }
}