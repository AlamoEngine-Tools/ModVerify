using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using AET.ModVerify.Reporting;
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
            AddError(VerificationError.Create(this, VerifierErrorCodes.FilePathTooLong,
                $"Could not find texture '{pathString}' because the engine resolved a path that is too long.",
                VerificationSeverity.Error, contextInfo, pathString));
            return;
        }


        var messageBuilder = new StringBuilder($"Could not find texture '{pathString}'");
        if (contextInfo.Count > 0)
        {
            messageBuilder.Append(" for context: [");
            messageBuilder.Append(string.Join("-->", contextInfo));
            messageBuilder.Append(']');
        }

        messageBuilder.Append('.');

        AddError(VerificationError.Create(this, VerifierErrorCodes.FileNotFound, 
            messageBuilder.ToString(),
            VerificationSeverity.Error, contextInfo, pathString));
    }
}