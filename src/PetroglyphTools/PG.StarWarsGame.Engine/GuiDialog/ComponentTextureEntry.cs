using System.Diagnostics;

namespace PG.StarWarsGame.Engine.GuiDialog;

[DebuggerDisplay("Type:{ComponentType}, Texture: {Texture}")]
public readonly struct ComponentTextureEntry(GuiComponentType componentType, string texture, bool isOverride)
{
    public string Texture { get; } = texture;
    
    public GuiComponentType ComponentType { get; } = componentType;

    public bool IsOverride { get; } = isOverride;
}