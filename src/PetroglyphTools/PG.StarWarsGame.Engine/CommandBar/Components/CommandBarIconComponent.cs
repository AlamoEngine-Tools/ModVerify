using PG.StarWarsGame.Engine.CommandBar.Xml;

namespace PG.StarWarsGame.Engine.CommandBar.Components;

public class CommandBarIconComponent(CommandBarComponentData xmlData) : CommandBarBaseComponent(xmlData)
{
    public override CommandBarComponentType Type => CommandBarComponentType.Icon;
}