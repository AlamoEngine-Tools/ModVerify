using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.GuiDialog;

public sealed class TypeBasedComponentTextureEntryComparer : IEqualityComparer<ComponentTextureEntry>
{
    public static readonly TypeBasedComponentTextureEntryComparer Instance = new();

    private TypeBasedComponentTextureEntryComparer()
    {
    }

    public bool Equals(ComponentTextureEntry x, ComponentTextureEntry y)
    {
        return x.ComponentType.Equals(y.ComponentType);
    }

    public int GetHashCode(ComponentTextureEntry obj)
    {
        return obj.ComponentType.GetHashCode();
    }
}