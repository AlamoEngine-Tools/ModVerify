using PG.StarWarsGame.Engine.CommandBar.Xml;

namespace PG.StarWarsGame.Engine.CommandBar.Components;

public class CommandBarShellComponent(CommandBarComponentData xmlData) : CommandBarBaseComponent(xmlData)
{
    public override CommandBarComponentType Type => CommandBarComponentType.Shell;

    public string? ModelName { get; } = xmlData.ModelName;
}