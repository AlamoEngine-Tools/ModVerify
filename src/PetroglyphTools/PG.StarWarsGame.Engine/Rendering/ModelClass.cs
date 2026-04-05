using System;
using System.Diagnostics.CodeAnalysis;
using AnakinRaW.CommonUtilities;
using PG.StarWarsGame.Engine.Rendering.Animations;
using PG.StarWarsGame.Engine.Utilities;
using PG.StarWarsGame.Files.ALO.Data;
using PG.StarWarsGame.Files.ALO.Files;

namespace PG.StarWarsGame.Engine.Rendering;

public sealed class ModelClass(
    IAloFile<IAloDataContent, AloFileInformation> aloFile,
    AnimationCollection animations)
    : DisposableObject
{
    public IAloDataContent RenderableContent { get; } = aloFile.Content;

    public IAloFile<IAloDataContent, AloFileInformation> File { get; } =
        aloFile ?? throw new ArgumentNullException(nameof(aloFile));

    public AlamoModel? Model { get; } = aloFile.Content as AlamoModel;

    [MemberNotNullWhen(true, nameof(Model))]
    public bool IsModel => Model is not null;

    public AnimationCollection Animations { get; } = animations ?? throw new ArgumentNullException(nameof(animations));

    public ModelClass(IAloFile<IAloDataContent, AloFileInformation> aloFile) : this(aloFile, AnimationCollection.Empty)
    {
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

    public static ReadOnlySpan<char> GetProxyName(ReadOnlySpan<char> proxyName)
    {
        var altIndex = ParseAlternateIndex(proxyName);
        if (altIndex == -1)
            return proxyName;

        var result = proxyName;

        var lodIndex = result.IndexOf("_LOD".AsSpan(), StringComparison.Ordinal);
        if (lodIndex != -1)
            result = result.Slice(0, lodIndex);

        var altIndexPos = result.IndexOf("_ALT".AsSpan(), StringComparison.Ordinal);
        if (altIndexPos != -1)
            result = result.Slice(0, altIndexPos);

        return result;
    }

    public static int ParseAlternateIndex(ReadOnlySpan<char> name)
    {
        const string altSuffix = "_ALT";

        var index = name.IndexOf(altSuffix.AsSpan(), StringComparison.Ordinal);
        if (index == -1)
            return -1;

        var startIndex = index + altSuffix.Length;
        if (startIndex >= name.Length)
            return -1;

        var remaining = name.Slice(startIndex);
        return remaining.Atoi();
    }
}