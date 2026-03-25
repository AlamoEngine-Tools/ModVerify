using AnakinRaW.CommonUtilities.Collections;
using PG.StarWarsGame.Engine.GuiDialog;
using PG.StarWarsGame.Engine.GuiDialog.Xml;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal class GuiDialogParser(IServiceProvider serviceProvider, IXmlParserErrorReporter? errorReporter = null) : 
    XmlFileParser<GuiDialogsXml>(serviceProvider, errorReporter)
{ 
    protected override GuiDialogsXml ParseRoot(XElement element, string fileName)
    {
        using var elementsEnumerator = element.Elements().GetEnumerator();

        var texturesExist = elementsEnumerator.MoveNext();
        var textures = ParseTextures(texturesExist
            ? elementsEnumerator.Current 
            : null!, 
            fileName);
        
        return new GuiDialogsXml(textures, XmlLocationInfo.FromElement(element));
    }

    private GuiDialogsXmlTextureData ParseTextures(XElement? element, string fileName)
    {
        if (element is null)
        {
            ErrorReporter?.Report(new XmlError(this, locationInfo: new XmlLocationInfo(fileName, null))
            {
                ErrorKind = XmlParseErrorKind.MissingNode,
                Message = "Unable to read textures for GUI."
            });
            return new GuiDialogsXmlTextureData([], new XmlLocationInfo(fileName, null));
        }

        if (element.Name != "Textures")
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                ErrorKind = XmlParseErrorKind.UnexceptedElementName,
                Message = "Unable to read textures for GUI."
            });
        }

        var textures = new List<XmlComponentTextureData>();

        GetAttributeValue(element, "File", out var megaTexture);
        GetAttributeValue(element, "Compressed_File", out var compressedMegaTexture);

        foreach (var texture in element.Elements()) 
            textures.Add(ParseTexture(texture));

        if (textures.Count == 0)
        {
            ErrorReporter?.Report(new XmlError(this, element)
            {
                Message = "Missing default texture specifications in GUI XML file!",
                ErrorKind = XmlParseErrorKind.MissingNode
            });
        }
        
        return new GuiDialogsXmlTextureData(textures, XmlLocationInfo.FromElement(element))
        {
            MegaTexture = megaTexture,
            CompressedMegaTexture = compressedMegaTexture
        };
    }

    private XmlComponentTextureData ParseTexture(XElement texture)
    {
        var componentId = GetTagName(texture);
        var textures = new FrugalValueListDictionary<string, string>();

        foreach (var entry in texture.Elements()) 
            textures.Add(entry.Name.ToString(), PetroglyphXmlStringParser.Instance.Parse(entry));
        
        return new XmlComponentTextureData(componentId, textures, XmlLocationInfo.FromElement(texture));
    }


    internal static readonly EnumConversionDictionary<GuiComponentType> ComponentTypeDictionary = new([
        new("Button_Left", GuiComponentType.ButtonLeft),
        new("Button_Middle", GuiComponentType.ButtonMiddle),
        new("Button_Right", GuiComponentType.ButtonRight),
        new("Button_Left_Mouse_Over", GuiComponentType.ButtonLeftMouseOver),
        new("Button_Middle_Mouse_Over", GuiComponentType.ButtonMiddleMouseOver),
        new("Button_Right_Mouse_Over", GuiComponentType.ButtonRightMouseOver),
        new("Button_Left_Pressed", GuiComponentType.ButtonLeftPressed),
        new("Button_Middle_Pressed", GuiComponentType.ButtonMiddlePressed),
        new("Button_Right_Pressed", GuiComponentType.ButtonRightPressed),
        new("Button_Left_Disabled", GuiComponentType.ButtonLeftDisabled),
        new("Button_Middle_Disabled", GuiComponentType.ButtonMiddleDisabled),
        new("Button_Right_Disabled", GuiComponentType.ButtonRightDisabled),

        new("Check_Off", GuiComponentType.CheckOff),
        new("Check_On", GuiComponentType.CheckOn),

        new("Dial_Left", GuiComponentType.DialLeft),
        new("Dial_Middle", GuiComponentType.DialMiddle),
        new("Dial_Right", GuiComponentType.DialRight),
        new("Dial_Plus", GuiComponentType.DialPlus),
        new("Dial_Plus_Mouse_Over", GuiComponentType.DialPlusMouseOver),
        new("Dial_Plus_Pressed", GuiComponentType.DialPlusPressed),
        new("Dial_Minus", GuiComponentType.DialMinus),
        new("Dial_Minus_Mouse_Over", GuiComponentType.DialMinusMouseOver),
        new("Dial_Minus_Pressed", GuiComponentType.DialMinusPressed),
        new("Dial_Tab", GuiComponentType.DialTab),
        
        new("Frame_Bottom", GuiComponentType.FrameBottom),
        new("Frame_Bottom_Left", GuiComponentType.FrameBottomLeft),
        new("Frame_Bottom_Right", GuiComponentType.FrameBottomRight),
        new("Frame_Background", GuiComponentType.FrameBackground),
        new("Frame_Left", GuiComponentType.FrameLeft),
        new("Frame_Right", GuiComponentType.FrameRight),
        new("Frame_Top", GuiComponentType.FrameTop),
        new("Frame_Top_Left", GuiComponentType.FrameTopLeft),
        new("Frame_Top_Right", GuiComponentType.FrameTopRight),
        new("Frame_Top_Transition_Left", GuiComponentType.FrameTopTransitionLeft),
        new("Frame_Top_Transition_Right", GuiComponentType.FrameTopTransitionRight),
        new("Frame_Bottom_Transition_Left", GuiComponentType.FrameBottomTransitionLeft),
        new("Frame_Bottom_Transition_Right", GuiComponentType.FrameBottomTransitionRight),
        new("Frame_Left_Transition_Top", GuiComponentType.FrameLeftTransitionTop),
        new("Frame_Left_Transition_Bottom", GuiComponentType.FrameLeftTransitionBottom),
        new("Frame_Right_Transition_Top", GuiComponentType.FrameRightTransitionTop),
        new("Frame_Right_Transition_Bottom", GuiComponentType.FrameRightTransitionBottom),
        
        new("Radio_Off", GuiComponentType.RadioOff),
        new("Radio_On", GuiComponentType.RadioOn),
        new("Radio_Disabled", GuiComponentType.RadioDisabled),
        new("Radio_Mouse_Over", GuiComponentType.RadioMouseOver),
        
        new("Scroll_Down_Button", GuiComponentType.ScrollDownButton),
        new("Scroll_Down_Button_Pressed", GuiComponentType.ScrollDownButtonPressed),
        new("Scroll_Down_Button_Mouse_Over", GuiComponentType.ScrollDownButtonMouseOver),
        new("Scroll_Down_Button_Disabled", GuiComponentType.ScrollDownButtonDisabled),
        new("Scroll_Middle", GuiComponentType.ScrollMiddle),
        new("Scroll_Middle_Disabled", GuiComponentType.ScrollMiddleDisabled),
        new("Scroll_Tab", GuiComponentType.ScrollTab),
        new("Scroll_Tab_Disabled", GuiComponentType.ScrollTabDisabled),
        new("Scroll_Up_Button", GuiComponentType.ScrollUpButton),
        new("Scroll_Up_Button_Pressed", GuiComponentType.ScrollUpButtonPressed),
        new("Scroll_Up_Button_Mouse_Over", GuiComponentType.ScrollUpButtonMouseOver),
        new("Scroll_Up_Button_Disabled", GuiComponentType.ScrollUpButtonDisabled),
        
        new("Trackbar_Scroll_Down_Button", GuiComponentType.TrackbarScrollDownButton),
        new("Trackbar_Scroll_Down_Button_Pressed", GuiComponentType.TrackbarScrollDownButtonPressed),
        new("Trackbar_Scroll_Down_Button_Mouse_Over", GuiComponentType.TrackbarScrollDownButtonMouseOver),
        new("Trackbar_Scroll_Down_Button_Disabled", GuiComponentType.TrackbarScrollDownButtonDisabled),
        new("Trackbar_Scroll_Middle", GuiComponentType.TrackbarScrollMiddle),
        new("Trackbar_Scroll_Middle_Disabled", GuiComponentType.TrackbarScrollMiddleDisabled),
        new("Trackbar_Scroll_Tab", GuiComponentType.TrackbarScrollTab),
        new("Trackbar_Scroll_Tab_Disabled", GuiComponentType.TrackbarScrollTabDisabled),
        new("Trackbar_Scroll_Up_Button", GuiComponentType.TrackbarScrollUpButton),
        new("Trackbar_Scroll_Up_Button_Pressed", GuiComponentType.TrackbarScrollUpButtonPressed),
        new("Trackbar_Scroll_Up_Button_Mouse_Over", GuiComponentType.TrackbarScrollUpButtonMouseOver),
        new("Trackbar_Scroll_Up_Button_Disabled", GuiComponentType.TrackbarScrollUpButtonDisabled),
        
        new("Small_Frame_Bottom", GuiComponentType.SmallFrameBottom),
        new("Small_Frame_Bottom_Left", GuiComponentType.SmallFrameBottomLeft),
        new("Small_Frame_Bottom_Right", GuiComponentType.SmallFrameBottomRight),
        new("Small_Frame_Left", GuiComponentType.SmallFrameMiddleLeft),
        new("Small_Frame_Right", GuiComponentType.SmallFrameMiddleRight),
        new("Small_Frame_Top", GuiComponentType.SmallFrameTop),
        new("Small_Frame_Top_Left", GuiComponentType.SmallFrameTopLeft),
        new("Small_Frame_Top_Right", GuiComponentType.SmallFrameTopRight),
        new("Small_Frame_Background", GuiComponentType.SmallFrameBackground),
        
        new("Combo_Box_Popdown_Button", GuiComponentType.ComboboxPopdown),
        new("Combo_Box_Popdown_Button_Pressed", GuiComponentType.ComboboxPopdownPressed),
        new("Combo_Box_Popdown_Button_Mouse_Over", GuiComponentType.ComboboxPopdownMouseOver),
        new("Combo_Box_Text_Box", GuiComponentType.ComboboxTextBox),
        new("Combo_Box_Left_Cap", GuiComponentType.ComboboxLeftCap),
        
        new("Progress_Bar_Left", GuiComponentType.ProgressLeft),
        new("Progress_Bar_Middle_Off", GuiComponentType.ProgressMiddleOff),
        new("Progress_Bar_Middle_On", GuiComponentType.ProgressMiddleOn),
        new("Progress_Bar_Right", GuiComponentType.ProgressRight),
        
        new("Scanlines", GuiComponentType.Scanlines),
    ]);
}