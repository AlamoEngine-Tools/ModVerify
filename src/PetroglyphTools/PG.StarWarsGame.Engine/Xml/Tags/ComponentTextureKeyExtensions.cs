using System;
using PG.StarWarsGame.Engine.GuiDialog;

namespace PG.StarWarsGame.Engine.Xml.Tags;

internal static class ComponentTextureKeyExtensions
{ 
    public static bool TryConvertToKey(ReadOnlySpan<char> keyValue, out GuiComponentType key)
    {
        key = keyValue switch
        {
            "Button_Left" => GuiComponentType.ButtonLeft,
            "Button_Middle" => GuiComponentType.ButtonMiddle,
            "Button_Right" => GuiComponentType.ButtonRight,
            "Button_Left_Mouse_Over" => GuiComponentType.ButtonLeftMouseOver,
            "Button_Middle_Mouse_Over" => GuiComponentType.ButtonMiddleMouseOver,
            "Button_Right_Mouse_Over" => GuiComponentType.ButtonRightMouseOver,
            "Button_Left_Pressed" => GuiComponentType.ButtonLeftPressed,
            "Button_Middle_Pressed" => GuiComponentType.ButtonMiddlePressed,
            "Button_Right_Pressed" => GuiComponentType.ButtonRightPressed,
            "Button_Left_Disabled" => GuiComponentType.ButtonLeftDisabled,
            "Button_Middle_Disabled" => GuiComponentType.ButtonMiddleDisabled,
            "Button_Right_Disabled" => GuiComponentType.ButtonRightDisabled,

            "Check_Off" => GuiComponentType.CheckOff,
            "Check_On" => GuiComponentType.CheckOn,

            "Dial_Left" => GuiComponentType.DialLeft,
            "Dial_Middle" => GuiComponentType.DialMiddle,
            "Dial_Right" => GuiComponentType.DialRight,
            "Dial_Plus" => GuiComponentType.DialPlus,
            "Dial_Plus_Mouse_Over" => GuiComponentType.DialPlusMouseOver,
            "Dial_Plus_Pressed" => GuiComponentType.DialPlusPressed,
            "Dial_Minus" => GuiComponentType.DialMinus,
            "Dial_Minus_Mouse_Over" => GuiComponentType.DialMinusMouseOver,
            "Dial_Minus_Pressed" => GuiComponentType.DialMinusPressed,
            "Dial_Tab" => GuiComponentType.DialTab,

            "Frame_Bottom" => GuiComponentType.FrameBottom,
            "Frame_Bottom_Left" => GuiComponentType.FrameBottomLeft,
            "Frame_Bottom_Right" => GuiComponentType.FrameBottomRight,
            "Frame_Background" => GuiComponentType.FrameBackground,
            "Frame_Left" => GuiComponentType.FrameLeft,
            "Frame_Right" => GuiComponentType.FrameRight,
            "Frame_Top" => GuiComponentType.FrameTop,
            "Frame_Top_Left" => GuiComponentType.FrameTopLeft,
            "Frame_Top_Right" => GuiComponentType.FrameTopRight,
            "Frame_Top_Transition_Left" => GuiComponentType.FrameTopTransitionLeft,
            "Frame_Top_Transition_Right" => GuiComponentType.FrameTopTransitionRight,
            "Frame_Bottom_Transition_Left" => GuiComponentType.FrameBottomTransitionLeft,
            "Frame_Bottom_Transition_Right" => GuiComponentType.FrameBottomTransitionRight,
            "Frame_Left_Transition_Top" => GuiComponentType.FrameLeftTransitionTop,
            "Frame_Left_Transition_Bottom" => GuiComponentType.FrameLeftTransitionBottom,
            "Frame_Right_Transition_Top" => GuiComponentType.FrameRightTransitionTop,
            "Frame_Right_Transition_Bottom" => GuiComponentType.FrameRightTransitionBottom,

            "Radio_Off" => GuiComponentType.RadioOff,
            "Radio_On" => GuiComponentType.RadioOn,
            "Radio_Disabled" => GuiComponentType.RadioDisabled,
            "Radio_Mouse_Over" => GuiComponentType.RadioMouseOver,

            "Scroll_Down_Button" => GuiComponentType.ScrollDownButton,
            "Scroll_Down_Button_Pressed" => GuiComponentType.ScrollDownButtonPressed,
            "Scroll_Down_Button_Mouse_Over" => GuiComponentType.ScrollDownButtonMouseOver,
            "Scroll_Down_Button_Disabled" => GuiComponentType.ScrollDownButtonDisabled,
            "Scroll_Middle" => GuiComponentType.ScrollMiddle,
            "Scroll_Middle_Disabled" => GuiComponentType.ScrollMiddleDisabled,
            "Scroll_Tab" => GuiComponentType.ScrollTab,
            "Scroll_Tab_Disabled" => GuiComponentType.ScrollTabDisabled,
            "Scroll_Up_Button" => GuiComponentType.ScrollUpButton,
            "Scroll_Up_Button_Pressed" => GuiComponentType.ScrollUpButtonPressed,
            "Scroll_Up_Button_Mouse_Over" => GuiComponentType.ScrollUpButtonMouseOver,
            "Scroll_Up_Button_Disabled" => GuiComponentType.ScrollUpButtonDisabled,

            "Trackbar_Scroll_Down_Button" => GuiComponentType.TrackbarScrollDownButton,
            "Trackbar_Scroll_Down_Button_Pressed" => GuiComponentType.TrackbarScrollDownButtonPressed,
            "Trackbar_Scroll_Down_Button_Mouse_Over" => GuiComponentType.TrackbarScrollDownButtonMouseOver,
            "Trackbar_Scroll_Down_Button_Disabled" => GuiComponentType.TrackbarScrollDownButtonDisabled,
            "Trackbar_Scroll_Middle" => GuiComponentType.TrackbarScrollMiddle,
            "Trackbar_Scroll_Middle_Disabled" => GuiComponentType.TrackbarScrollMiddleDisabled,
            "Trackbar_Scroll_Tab" => GuiComponentType.TrackbarScrollTab,
            "Trackbar_Scroll_Tab_Disabled" => GuiComponentType.TrackbarScrollTabDisabled,
            "Trackbar_Scroll_Up_Button" => GuiComponentType.TrackbarScrollUpButton,
            "Trackbar_Scroll_Up_Button_Pressed" => GuiComponentType.TrackbarScrollUpButtonPressed,
            "Trackbar_Scroll_Up_Button_Mouse_Over" => GuiComponentType.TrackbarScrollUpButtonMouseOver,
            "Trackbar_Scroll_Up_Button_Disabled" => GuiComponentType.TrackbarScrollUpButtonDisabled,

            "Small_Frame_Bottom" => GuiComponentType.SmallFrameBottom,
            "Small_Frame_Bottom_Left" => GuiComponentType.SmallFrameBottomLeft,
            "Small_Frame_Bottom_Right" => GuiComponentType.SmallFrameBottomRight,
            "Small_Frame_Left" => GuiComponentType.SmallFrameMiddleLeft,
            "Small_Frame_Right" => GuiComponentType.SmallFrameMiddleRight,
            "Small_Frame_Top" => GuiComponentType.SmallFrameTop,
            "Small_Frame_Top_Left" => GuiComponentType.SmallFrameTopLeft,
            "Small_Frame_Top_Right" => GuiComponentType.SmallFrameTopRight,
            "Small_Frame_Background" => GuiComponentType.SmallFrameBackground,

            "Combo_Box_Popdown_Button" => GuiComponentType.ComboboxPopdown,
            "Combo_Box_Popdown_Button_Pressed" => GuiComponentType.ComboboxPopdownPressed,
            "Combo_Box_Popdown_Button_Mouse_Over" => GuiComponentType.ComboboxPopdownMouseOver,
            "Combo_Box_Text_Box" => GuiComponentType.ComboboxTextBox,
            "Combo_Box_Left_Cap" => GuiComponentType.ComboboxLeftCap,

            "Progress_Bar_Left" => GuiComponentType.ProgressLeft,
            "Progress_Bar_Middle_Off" => GuiComponentType.ProgressMiddleOff,
            "Progress_Bar_Middle_On" => GuiComponentType.ProgressMiddleOn,
            "Progress_Bar_Right" => GuiComponentType.ProgressRight,

            "Scanlines" => GuiComponentType.Scanlines,
            _ => (GuiComponentType)int.MaxValue
        };
        return (int)key != int.MaxValue;
    }
}