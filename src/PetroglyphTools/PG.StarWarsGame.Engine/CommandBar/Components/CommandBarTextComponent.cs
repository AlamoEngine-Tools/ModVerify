using PG.StarWarsGame.Engine.CommandBar.Xml;
using PG.StarWarsGame.Engine.Rendering;

namespace PG.StarWarsGame.Engine.CommandBar.Components;

public class CommandBarTextComponent : CommandBarBaseComponent
{
    public override CommandBarComponentType Type => CommandBarComponentType.Text;

    public CommandBarComponentTextData TextData { get; }

    public CommandBarComponentTextData FlashData { get; }

    public bool AutoPixelAlign => true;


    public CommandBarTextComponent(CommandBarComponentData xmlData) : base(xmlData)
    {
        TextData = new CommandBarComponentTextData(this)
        {
            Scale = XmlData.Scale,
            Color = Color,
            TempAlpha = -1,
            Mode = PrimRenderMode.PrimAlpha,
        };
        FlashData = new CommandBarComponentTextData(this);
    }
}