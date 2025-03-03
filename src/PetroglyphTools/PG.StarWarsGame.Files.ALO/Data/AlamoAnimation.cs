using System.Collections.Generic;

namespace PG.StarWarsGame.Files.ALO.Data;

public sealed class AlamoAnimation : IAloDataContent
{
    public float FPS { get; init; }

    public uint NumberFrames { get; init; }

    public uint NumberBones { get; init; }

    public IReadOnlyCollection<AnimationBoneData> BoneData { get; init; }

    public void Dispose()
    {
    }
}

public readonly struct AnimationBoneData(uint index, string name)
{
    public uint BoneIndex { get; } = index;

    public string Name { get; } = name;
}
