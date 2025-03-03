using System;
using System.Diagnostics.CodeAnalysis;
using AnakinRaW.CommonUtilities;
using PG.StarWarsGame.Files;
using PG.StarWarsGame.Files.ALO.Data;
using PG.StarWarsGame.Files.ALO.Files;

namespace PG.StarWarsGame.Engine.Rendering;

public sealed class ModelClass : DisposableObject
{
    public IAloDataContent RenderableContent { get; }

    public IPetroglyphFileHolder File { get; }

    public AlamoModel? Model { get; }

    [MemberNotNullWhen(true, nameof(Model))]
    public bool IsModel => Model is not null;

    public AnimationCollection Animations { get; } = new();

    public ModelClass(IAloFile<IAloDataContent, AloFileInformation> aloFile)
    {
        File = aloFile;
        RenderableContent = aloFile.Content;
        Model = aloFile.Content as AlamoModel;
    }

    public int IndexOfBone(string boneName)
    {
        if (!IsModel)
            return -1;
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
        File.Dispose();
        Animations.Dispose();
    }
}