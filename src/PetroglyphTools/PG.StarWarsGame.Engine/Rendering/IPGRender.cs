using PG.StarWarsGame.Files.ALO.Data;
using PG.StarWarsGame.Files.ALO.Files;
using PG.StarWarsGame.Files.Binary;
using System;
using PG.StarWarsGame.Engine.Rendering.Animations;

namespace PG.StarWarsGame.Engine.Rendering;

public interface IPGRender
{
    IAloFile<IAloDataContent, AloFileInformation>? Load3DAsset(
        string path, 
        bool metadataOnly = true, 
        bool throwsException = false);

    IAloFile<IAloDataContent, AloFileInformation>? Load3DAsset(
        ReadOnlySpan<char> path, 
        bool metadataOnly = true, 
        bool throwsException = false);

    ModelClass? LoadModelAndAnimations(
        ReadOnlySpan<char> path, 
        string? animOverrideName,
        bool metadataOnly = true, 
        bool throwsException = false);

    public AnimationCollection LoadAnimations(
        ReadOnlySpan<char> fileName,
        ReadOnlySpan<char> dirPath,
        bool metadataOnly = true,
        Action<BinaryCorruptedException>? corruptedAnimationHandler = null);
}