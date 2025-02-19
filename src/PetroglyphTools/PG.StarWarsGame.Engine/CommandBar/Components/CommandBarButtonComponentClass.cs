using PG.StarWarsGame.Engine.CommandBar.Xml;

namespace PG.StarWarsGame.Engine.CommandBar.Components;

public class CommandBarButtonComponentClass(CommandBarComponentData xmlData) : CommandBarIconComponent(xmlData)
{
    public override CommandBarComponentType Type => CommandBarComponentType.Button;

    public ButtonMode Mode => ButtonMode.Normal;
}