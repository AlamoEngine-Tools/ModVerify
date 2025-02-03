namespace PG.StarWarsGame.Engine.GuiDialog;

public readonly struct ComponentTextureEntry(GuiComponentType componentType, string texture, bool isOverride)
{
    public string Texture { get; } = texture;
    
    public GuiComponentType ComponentType { get; } = componentType;

    public bool IsOverride { get; } = isOverride;
}