using System;
using PG.StarWarsGame.Files.ALO.Data;
using PG.StarWarsGame.Files.ALO.Files;

namespace PG.StarWarsGame.Engine.Rendering;

public interface IPGRender
{
    IAloFile<IAloDataContent, AloFileInformation>? Load3DAsset(string path, bool metadataOnly = true);

    IAloFile<IAloDataContent, AloFileInformation>? Load3DAsset(ReadOnlySpan<char> path, bool metadataOnly = true);

    ModelClass? LoadModelAndAnimations(ReadOnlySpan<char> path, string? animOverrideName, bool metadataOnly = true);
}