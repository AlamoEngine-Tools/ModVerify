using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.Database;

namespace AET.ModVerify.Verifiers.Commons;

public sealed class TextureVeifier(
    IGameVerifierInfo? parent, 
    IGameDatabase gameDatabase, 
    GameVerifySettings settings, 
    IServiceProvider serviceProvider) 
    : GameVerifier<string>(parent, gameDatabase, settings, serviceProvider)
{
    private readonly IAlreadyVerifiedCache? _cache = serviceProvider.GetService<IAlreadyVerifiedCache>();

    public override void Verify(string texturePath, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        Verify(texturePath.AsSpan(), contextInfo, token);
    }

    public void Verify(ReadOnlySpan<char> textureName, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

      
        if (_cache?.TryAddEntry(textureName) == false)
            return;

        if (Repository.TextureRepository.FileExists(textureName, false, out var tooLongPath))
            return;

        var pathString = textureName.ToString();

        if (tooLongPath)
        {
            VerificationError.Create(VerifierChain, VerifierErrorCodes.FilePathTooLong, 
                $"Could not find texture '{pathString}' because the engine resolved a path that is too long.",
                VerificationSeverity.Error, contextInfo, pathString);
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

        VerificationError.Create(VerifierChain, VerifierErrorCodes.FileNotFound, 
            messageBuilder.ToString(),
            VerificationSeverity.Error, contextInfo, pathString);
    }
}