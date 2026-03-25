namespace PG.StarWarsGame.Engine.GuiDialog;

public static class ExtensionMethods
{
    extension(GuiComponentType componentType)
    {
        public bool IsButton()
        {
            return componentType <= GuiComponentType.ButtonRightDisabled;
        }

        public bool SupportsSpecialTextureMode()
        {
            return componentType is GuiComponentType.ButtonMiddle
                or GuiComponentType.ButtonMiddleMouseOver
                or GuiComponentType.ButtonMiddlePressed
                or GuiComponentType.ButtonMiddleDisabled
                or GuiComponentType.Scanlines
                or GuiComponentType.FrameBackground;
        }
    }
}