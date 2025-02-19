using PG.StarWarsGame.Engine.CommandBar.Xml;
using PG.StarWarsGame.Engine.Rendering;

namespace PG.StarWarsGame.Engine.CommandBar.Components;

public class CommandBarTextButtonComponent : CommandBarButtonComponentClass
{
    public CommandBarComponentTextData TextData { get; }
    public CommandBarComponentTextData TextData2 { get; }
    public CommandBarComponentTextData FlashData { get; }
    public bool ShowDraggedText { get; } = true;
    public override CommandBarComponentType Type => CommandBarComponentType.TextButton;

    public CommandBarTextButtonComponent(CommandBarComponentData xmlData) : base(xmlData)
    {
        TextData = new CommandBarComponentTextData(this)
        {
            Mode = PrimRenderMode.PrimAlpha,
            Scale = xmlData.Scale,
            TempAlpha = -1,
            Color = xmlData.TextColor.HasValue ? new RgbaColor(xmlData.TextColor.Value) : new RgbaColor(255, 255, 255, 255)
        };
        TextData2 = new CommandBarComponentTextData(this)
        {
            Mode = PrimRenderMode.PrimAlpha,
            Scale = xmlData.Scale,
            TempAlpha = -1,
            Color = xmlData.TextColor2.HasValue ? new RgbaColor(xmlData.TextColor2.Value) : new RgbaColor(255, 255, 255, 255)
        };
        FlashData = new CommandBarComponentTextData(this);
    }
}