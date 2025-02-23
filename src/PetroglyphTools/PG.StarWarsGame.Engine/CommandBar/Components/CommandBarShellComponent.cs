using PG.StarWarsGame.Engine.CommandBar.Xml;

namespace PG.StarWarsGame.Engine.CommandBar.Components;

public class CommandBarShellComponent : CommandBarBaseComponent
{
    public override CommandBarComponentType Type => CommandBarComponentType.Shell;

    public string? ModelName { get; }

    public string? ModelPath { get; }

    public CommandBarShellComponent(CommandBarComponentData xmlData) : base(xmlData)
    {
        ModelName = xmlData.ModelName;
        if (!string.IsNullOrEmpty(ModelName))
            ModelPath = $"DATA\\ART\\MODELS\\{ModelName}";
    }
}