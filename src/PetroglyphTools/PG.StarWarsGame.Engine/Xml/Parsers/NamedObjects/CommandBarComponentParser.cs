using AnakinRaW.CommonUtilities.Collections;
using PG.StarWarsGame.Engine.CommandBar.Xml;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;
using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using Crc32 = PG.Commons.Hashing.Crc32;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal class CommandBarComponentParser(IServiceProvider serviceProvider, IXmlParserErrorReporter? errorReporter = null)
    : NamedXmlObjectParser<CommandBarComponentData>(serviceProvider, new CommandBarComponentDataXmlTagMapper(serviceProvider), errorReporter)
{
    protected override CommandBarComponentData CreateXmlObject(
        string name, 
        Crc32 nameCrc,
        XElement element,
        IReadOnlyFrugalValueListDictionary<Crc32, CommandBarComponentData> parsedEntries,
        XmlLocationInfo location)
    {
        return new CommandBarComponentData(name, nameCrc, location);
    }


    protected override void ValidateAndFixupValues(CommandBarComponentData xmlData, XElement element)
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
                (obj, val) => obj.SelectedTextureNames = new ReadOnlyCollection<string>(val));
            AddMapping(
                CommandBarComponentTags.BlankTextureName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val) => obj.BlankTextureNames = new ReadOnlyCollection<string>(val));
            AddMapping(
                CommandBarComponentTags.IconTextureName,
                PetroglyphXmlStringParser.Instance.Parse,
                (obj, val) => obj.IconTextureName = val);
            AddMapping(
                CommandBarComponentTags.IconAlternateTextureName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val) => obj.IconAlternateTextureNames = new ReadOnlyCollection<string>(val));
            AddMapping(
                CommandBarComponentTags.MouseOverTextureName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val) => obj.MouseOverTextureNames = new ReadOnlyCollection<string>(val));
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
                (obj, val) => obj.BarTextureNames = new ReadOnlyCollection<string>(val));
            AddMapping(
                CommandBarComponentTags.BarOverlayName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val) => obj.BarOverlayNames = new ReadOnlyCollection<string>(val));
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
                (obj, val) => obj.AlternateFontNames = new ReadOnlyCollection<string>(val));
            AddMapping(
                CommandBarComponentTags.TooltipText,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val) => obj.TooltipTexts = new ReadOnlyCollection<string>(val));
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
                (obj, val) => obj.LowerEffectTextureNames = new ReadOnlyCollection<string>(val));
            AddMapping(
                CommandBarComponentTags.UpperEffectTextureName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val) => obj.UpperEffectTextureNames = new ReadOnlyCollection<string>(val));
            AddMapping(
                CommandBarComponentTags.OverlayTextureName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val) => obj.OverlayTextureNames = new ReadOnlyCollection<string>(val));
            AddMapping(
                CommandBarComponentTags.Overlay2TextureName,
                PetroglyphXmlLooseStringListParser.Instance.Parse,
                (obj, val) => obj.Overlay2TextureNames = new ReadOnlyCollection<string>(val));
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

    internal static class CommandBarComponentTags
    {
        public const string SelectedTextureName = "Selected_Texture_Name";
        public const string BlankTextureName = "Blank_Texture_Name";
        public const string IconTextureName = "Icon_Texture_Name";
        public const string IconAlternateTextureName = "Icon_Alternate_Texture_Name";
        public const string MouseOverTextureName = "Mouse_Over_Texture_Name";
        public const string DisabledTextureName = "Disabled_Texture_Name";
        public const string FlashTextureName = "Flash_Texture_Name";
        public const string BarTextureName = "Bar_Texture_Name";
        public const string BarOverlayName = "Bar_Overlay_Name";
        public const string BuildTextureName = "Build_Texture_Name";
        public const string ModelName = "Model_Name";
        public const string BoneName = "Bone_Name";
        public const string CursorTextureName = "Cursor_Texture_Name";
        public const string FontName = "Font_Name";
        public const string AlternateFontName = "Alternate_Font_Name";
        public const string TooltipText = "Tooltip_Text";
        public const string ClickSfx = "Click_SFX";
        public const string MouseOverSfx = "Mouse_Over_SFX";
        public const string LowerEffectTextureName = "Lower_Effect_Texture_Name";
        public const string UpperEffectTextureName = "Upper_Effect_Texture_Name";
        public const string OverlayTextureName = "Overlay_Texture_Name";
        public const string Overlay2TextureName = "Overlay2_Texture_Name";
        public const string RightClickSfx = "Right_Click_SFX";
        public const string Type = "Type";
        public const string Group = "Group";
        public const string DragAndDrop = "Drag_And_Drop";
        public const string DragSelect = "Drag_Select";
        public const string Receptor = "Receptor";
        public const string Toggle = "Toggle";
        public const string Tab = "Tab";
        public const string AssociatedText = "Associated_Text";
        public const string Hidden = "Hidden";
        public const string Scale = "Scale";
        public const string Color = "Color";
        public const string TextColor = "Text_Color";
        public const string TextColor2 = "Text_Color2";
        public const string Size = "Size";
        public const string ClearColor = "Clear_Color";
        public const string Disabled = "Disabled";
        public const string SwapTexture = "Swap_Texture";
        public const string BaseLayer = "Base_Layer";
        public const string DrawAdditive = "Draw_Additive";
        public const string TextOffset = "Text_Offset";
        public const string TextOffset2 = "Text_Offset2";
        public const string Offset = "Offset";
        public const string DefaultOffset = "Default_Offset";
        public const string DefaultOffsetWidescreen = "Default_Offset_Widescreen";
        public const string IconOffset = "Icon_Offset";
        public const string MouseOverOffset = "Mouse_Over_Offset";
        public const string DisabledOffset = "Disabled_Offset";
        public const string BuildDialOffset = "Build_Dial_Offset";
        public const string BuildDial2Offset = "Build_Dial2_Offset";
        public const string LowerEffectOffset = "Lower_Effect_Offset";
        public const string UpperEffectOffset = "Upper_Effect_Offset";
        public const string OverlayOffset = "Overlay_Offset";
        public const string Overlay2Offset = "Overlay2_Offset";
        public const string Editable = "Editable";
        public const string MaxTextLength = "Max_Text_Length";
        public const string BlinkRate = "Blink_Rate";
        public const string FontPointSize = "Font_Point_Size";
        public const string TextOutline = "Text_Outline";
        public const string MaxTextWidth = "Max_Text_Width";
        public const string Stackable = "Stackable";
        public const string ModelOffsetX = "Model_Offset_X";
        public const string ModelOffsetY = "Model_Offset_Y";
        public const string ScaleModelX = "Scale_Model_X";
        public const string ScaleModelY = "Scale_Model_Y";
        public const string Collideable = "Collideable";
        public const string TextEmboss = "Text_Emboss";
        public const string ShouldGhost = "Should_Ghost";
        public const string GhostBaseOnly = "Ghost_Base_Only";
        public const string MaxBarLevel = "Max_Bar_Level";
        public const string MaxBarColor = "Max_Bar_Color";
        public const string CrossFade = "Cross_Fade";
        public const string LeftJustified = "Left_Justified";
        public const string RightJustified = "Right_Justified";
        public const string NoShell = "No_Shell";
        public const string SnapDrag = "Snap_Drag";
        public const string SnapLocation = "Snap_Location";
        public const string BlinkDuration = "Blink_Duration";
        public const string ScaleDuration = "Scale_Duration";
        public const string OffsetRender = "Offset_Render";
        public const string BlinkFade = "Blink_Fade";
        public const string NoHiddenCollision = "No_Hidden_Collision";
        public const string ManualOffset = "Manual_Offset";
        public const string SelectedAlpha = "Selected_Alpha";
        public const string PixelAlign = "Pixel_Align";
        public const string CanDragStack = "Can_Drag_Stack";
        public const string CanAnimate = "Can_Animate";
        public const string AnimFps = "Anim_FPS";
        public const string LoopAnim = "Loop_Anim";
        public const string SmoothBar = "Smooth_Bar";
        public const string OutlinedBar = "Outlined_Bar";
        public const string DragBack = "Drag_Back";
        public const string LowerEffectAdditive = "Lower_Effect_Additive";
        public const string UpperEffectAdditive = "Upper_Effect_Additive";
        public const string ClickShift = "Click_Shift";
        public const string TutorialScene = "Tutorial_Scene";
        public const string DialogScene = "Dialog_Scene";
        public const string ShouldRenderAtDragPos = "Should_Render_At_Drag_Pos";
        public const string DisableDarken = "Disable_Darken";
        public const string AnimateBack = "Animate_Back";
        public const string AnimateUpperEffect = "Animate_Upper_Effect";
    }
}