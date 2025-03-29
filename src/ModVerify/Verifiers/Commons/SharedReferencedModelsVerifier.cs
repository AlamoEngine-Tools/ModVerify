using System;
using System.Collections.Generic;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AET.ModVerify.Utilities;
using AnakinRaW.CommonUtilities.FileSystem;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Files;
using PG.StarWarsGame.Files.ALO.Data;
using PG.StarWarsGame.Files.ALO.Files;
using PG.StarWarsGame.Files.ALO.Files.Models;
using PG.StarWarsGame.Files.ALO.Files.Particles;
using PG.StarWarsGame.Files.Binary;
using PG.StarWarsGame.Files.ChunkFiles.Data;

namespace AET.ModVerify.Verifiers.Commons;

public sealed class SharedReferencedModelsVerifier(
    IGameVerifierInfo? parent,
    IEnumerable<string> modelSource,
    IGameDatabase database,
    GameVerifySettings settings,
    IServiceProvider serviceProvider)
    : GameVerifier(parent, database, settings, serviceProvider)
{
    private const string ProxyAltIdentifier = "_ALT";

    private readonly AlreadyVerifiedCache _cache = AlreadyVerifiedCache.Instance;

    public override string FriendlyName => "Models";

    public override void Verify(CancellationToken token)
    {
        var aloQueue = new Queue<string>(modelSource);

        while (aloQueue.Count != 0)
        {
            var fileName = aloQueue.Dequeue();
            if (!_cache.TryAddModel(fileName))
                continue;

            token.ThrowIfCancellationRequested();

            var modelPath = BuildModelPath(fileName).AsSpan();

            IAloFile<IAloDataContent, AloFileInformation>? aloFile = null;
            try
            {
                try
                {
                    aloFile = Database.PGRender.Load3DAsset(modelPath, true, true);
                }
                catch (BinaryCorruptedException e)
                {
                    var aloFilePath = FileSystem.Path.GetGameStrippedPath(Repository.Path.AsSpan(), modelPath).ToString();
                    var message = $"{aloFile} is corrupted: {e.Message}";
                    AddError(VerificationError.Create(VerifierChain, VerifierErrorCodes.ModelBroken, message, VerificationSeverity.Critical, aloFilePath));
                    continue;
                }

                if (aloFile is null)
                {
                    var error = VerificationError.Create(
                        VerifierChain,
                        VerifierErrorCodes.ModelNotFound,
                        $"Unable to find .ALO file '{fileName}'",
                        VerificationSeverity.Error,
                        fileName);
                    AddError(error);
                    continue;
                }

                VerifyModelOrParticle(aloFile, aloQueue);
            }
            finally
            {
                aloFile?.Dispose();
            }
        }
    }

    private void VerifyModelOrParticle(IAloFile<IAloDataContent, AloFileInformation> aloFile, Queue<string> workingQueue)
    {
        switch (aloFile)
        {
            case IAloModelFile model:
                VerifyModel(model, workingQueue);
                break;
            case IAloParticleFile particle:
                VerifyParticle(particle);
                break;
            default:
                throw new InvalidOperationException("The data stream is neither a model nor particle.");
        }
    }

    private void VerifyParticle(IAloParticleFile file)
    {
        foreach (var texture in file.Content.Textures)
        {
            GuardedVerify(() => VerifyTextureExists(file, texture),
                e => e is ArgumentException,
                _ =>
                {
                    var particlePath = FileSystem.Path.GetGameStrippedPath(Repository.Path.AsSpan(), file.FilePath.AsSpan()).ToString();
                    AddError(VerificationError.Create(
                        VerifierChain,
                        VerifierErrorCodes.InvalidTexture,
                        $"Invalid texture file name '{texture}' in particle '{particlePath}'",
                        VerificationSeverity.Error,
                        [particlePath], texture));
                });
        }

        var fileName = FileSystem.Path.GetFileNameWithoutExtension(file.FilePath.AsSpan());
        var name = file.Content.Name.AsSpan();

        if (!fileName.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            var particlePath = FileSystem.Path.GetGameStrippedPath(Repository.Path.AsSpan(), file.FilePath.AsSpan()).ToString();
            AddError(VerificationError.Create(
                VerifierChain,
                VerifierErrorCodes.InvalidParticleName,
                $"The particle name '{file.Content.Name}' does not match file name '{particlePath}'",
                VerificationSeverity.Error,
                particlePath));
        }

    }

    private void VerifyModel(IAloModelFile file, Queue<string> workingQueue)
    {
        foreach (var texture in file.Content.Textures)
        {
            GuardedVerify(() => VerifyTextureExists(file, texture),
                e => e is ArgumentException,
                _ =>
                {
                    var modelFilePath =
                        FileSystem.Path.GetGameStrippedPath(Repository.Path.AsSpan(), file.FilePath.AsSpan()).ToString();
                    AddError(VerificationError.Create(
                        VerifierChain,
                        VerifierErrorCodes.InvalidTexture,
                        $"Invalid texture file name '{texture}' in model '{modelFilePath}'",
                        VerificationSeverity.Error,
                        [modelFilePath], texture));
                });
        }

        foreach (var shader in file.Content.Shaders)
        {
            GuardedVerify(() => VerifyShaderExists(file, shader),
                e => e is ArgumentException,
                _ =>
                {
                    var modelFilePath =
                        FileSystem.Path.GetGameStrippedPath(Repository.Path.AsSpan(), file.FilePath.AsSpan()).ToString();
                    AddError(VerificationError.Create(
                        VerifierChain,
                        VerifierErrorCodes.InvalidShader,
                        $"Invalid texture file name '{shader}' in model '{modelFilePath}'",
                        VerificationSeverity.Error,
                        [modelFilePath],
                        shader));
                });
        }


        foreach (var proxy in file.Content.Proxies)
        {
            GuardedVerify(() => VerifyProxyExists(file, proxy, workingQueue),
                e => e is ArgumentException,
                _ =>
                {
                    var modelFilePath = FileSystem.Path
                        .GetGameStrippedPath(Repository.Path.AsSpan(), file.FilePath.AsSpan()).ToString();
                    AddError(VerificationError.Create(
                        VerifierChain,
                        VerifierErrorCodes.InvalidProxy,
                        $"Invalid proxy file name '{proxy}' in model '{modelFilePath}'",
                        VerificationSeverity.Error,
                        [modelFilePath],
                        proxy));
                });
        }
    }

    private void VerifyTextureExists(IPetroglyphFileHolder<IChunkData, PetroglyphFileInformation> model, string texture)
    {
        if (texture == "None")
            return;

        if (!Repository.TextureRepository.FileExists(texture))
        {
            var modelFilePath = FileSystem.Path.GetGameStrippedPath(Repository.Path.AsSpan(), model.FilePath.AsSpan())
                .ToString();
            var message = $"{modelFilePath} references missing texture: {texture}";
            var error = VerificationError.Create(VerifierChain, VerifierErrorCodes.ModelMissingTexture, message, VerificationSeverity.Error, [modelFilePath], texture);
            AddError(error);
        }
    }

    private void VerifyProxyExists(IPetroglyphFileHolder model, string proxy, Queue<string> workingQueue)
    {
        var proxyName = ProxyNameWithoutAlt(proxy);

        if (!Repository.ModelRepository.FileExists(BuildModelPath(proxyName)))
        {
            var modelFilePath = FileSystem.Path.GetGameStrippedPath(Repository.Path.AsSpan(), model.FilePath.AsSpan()).ToString();
            var message = $"{modelFilePath} references missing proxy particle: {proxyName}";
            var error = VerificationError.Create(VerifierChain, 
                VerifierErrorCodes.ModelMissingProxy, message, VerificationSeverity.Error, [modelFilePath], proxyName);
            AddError(error);
        }
        else
            workingQueue.Enqueue(proxyName);
    }

    private string BuildModelPath(string fileName)
    {
        return FileSystem.Path.Combine("DATA\\ART\\MODELS", fileName);
    }

    private void VerifyShaderExists(IPetroglyphFileHolder data, string shader)
    {
        if (shader is "alDefault.fx" or "alDefault.fxo")
            return;

        if (!Repository.EffectsRepository.FileExists(shader))
        {
            var modelFilePath = FileSystem.Path.GetGameStrippedPath(Repository.Path.AsSpan(), data.FilePath.AsSpan()).ToString();
            var message = $"{modelFilePath} references missing shader effect: {shader}";
            var error = VerificationError.Create(VerifierChain, VerifierErrorCodes.ModelMissingShader, message, VerificationSeverity.Error, [modelFilePath], shader);
            AddError(error);
        }
    }

    private static string ProxyNameWithoutAlt(string proxy)
    {
        var proxyName = proxy.AsSpan();

        var altSpan = ProxyAltIdentifier.AsSpan();

        var altIndex = proxyName.LastIndexOf(altSpan);

        if (altIndex == -1)
            return proxy;

        while (altIndex != -1)
        {
            proxyName = proxyName.Slice(0, altIndex);
            altIndex = proxyName.LastIndexOf(altSpan);
        }

        return proxyName.ToString();
    }
}