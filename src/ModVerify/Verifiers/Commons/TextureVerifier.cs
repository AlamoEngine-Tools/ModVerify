using System;
using System.Collections.Generic;
using System.Threading;
using Diagnostics = AET.ModVerify.Reporting.Diagnostics;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Verifiers.Commons;

public sealed class TextureVerifier : GameVerifier<string>
{
    public TextureVerifier(GameVerifierBase parent) : base(parent)
    {
    }

    public TextureVerifier(
        IGameVerifierInfo? parent,
        IStarWarsGameEngine gameEngine, 
        GameVerifySettings settings, 
        IServiceProvider serviceProvider) :
        base(parent, gameEngine, settings, serviceProvider)
    {
    }

    public override void Verify(string texturePath, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        Verify(texturePath.AsSpan(), contextInfo, token);
    }

    public void Verify(ReadOnlySpan<char> textureName, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (Repository.TextureRepository.FileExists(textureName, false, out var tooLongPath))
            return;

        var pathString = textureName.ToString();

        if (tooLongPath)
        {
            AddError(Diagnostics.Textures.PathTooLong(this, pathString, contextInfo));
            return;
        }

        AddError(Diagnostics.Textures.NotFound(this, pathString, contextInfo));
    }
}