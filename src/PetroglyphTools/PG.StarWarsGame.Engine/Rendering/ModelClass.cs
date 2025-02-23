using System;
using AnakinRaW.CommonUtilities;
using PG.StarWarsGame.Files.ALO.Data;

namespace PG.StarWarsGame.Engine.Rendering;

internal sealed class ModelClass(AlamoModel model) : DisposableObject
{
    public AlamoModel Model { get; } = model ?? throw new ArgumentNullException(nameof(model));

    public int IndexOfBone(string boneName)
    {
        var bones = Model.Bones;
        for (var i = 0; i < bones.Count; i++)
        {
            if (bones[i].Equals(boneName, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    protected override void DisposeResources()
    {
        Model.Dispose();
    }
}