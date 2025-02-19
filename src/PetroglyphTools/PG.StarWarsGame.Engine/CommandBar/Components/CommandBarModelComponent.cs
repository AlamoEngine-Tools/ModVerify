using PG.StarWarsGame.Engine.CommandBar.Xml;

namespace PG.StarWarsGame.Engine.CommandBar.Components;

public class CommandBarModelComponent(CommandBarComponentData xmlData) : CommandBarBaseComponent(xmlData)
{
    public override CommandBarComponentType Type => CommandBarComponentType.Model;
}