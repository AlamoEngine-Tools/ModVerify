using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AET.ModVerify.Utilities;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Files.ALO.Files.Models;
using PG.StarWarsGame.Files.ALO.Files.Particles;
using PG.StarWarsGame.Files.ChunkFiles.Data;
using AnakinRaW.CommonUtilities.FileSystem;
using PG.StarWarsGame.Files;
using PG.StarWarsGame.Files.ALO.Data;
using PG.StarWarsGame.Files.ALO.Files;
using PG.StarWarsGame.Files.Binary;

namespace AET.ModVerify.Verifiers.Commons;

internal sealed class AlreadyVerifiedCache
{
    internal static readonly AlreadyVerifiedCache Instance = new();

    private readonly ConcurrentDictionary<string, byte> _cachedModels = new(StringComparer.OrdinalIgnoreCase);

    private AlreadyVerifiedCache()
    {
    }

    public bool TryAddModel(string fileName)
    {
        return _cachedModels.TryAdd(fileName, 0);
    }
}

public sealed class SharedReferencedModelsVerifier(
    IGameVerifier? parent,
    IEnumerable<string> modelSource,
    IGameDatabase database,
    GameVerifySettings settings,
    IServiceProvider serviceProvider)
    : GameVerifierBase(parent, database, settings, serviceProvider)
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
                    var modelFilePath = FileSystem.Path.GetGameStrippedPath(Repository.Path.AsSpan(), file.FilePath.AsSpan()).ToString();
                    AddError(VerificationError.Create(
                        VerifierChain,
                        VerifierErrorCodes.InvalidTexture,
                        $"Invalid texture file name" +
                        $" '{texture}' in particle 'modelFilePath'",
                        VerificationSeverity.Error,
                        texture,
                        modelFilePath));
                });
        }

        var fileName = FileSystem.Path.GetFileNameWithoutExtension(file.FilePath.AsSpan());
        var name = file.Content.Name.AsSpan();

        if (!fileName.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            var modelFilePath = FileSystem.Path.GetGameStrippedPath(Repository.Path.AsSpan(), file.FilePath.AsSpan()).ToString();
            AddError(VerificationError.Create(
                VerifierChain,
                VerifierErrorCodes.InvalidParticleName,
                $"The particle name '{file.Content.Name}' does not match file name '{modelFilePath}'",
                VerificationSeverity.Error,
                modelFilePath));
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
                        texture, modelFilePath));
                });
        }

        foreach (var shader in file.Content.Shaders)
        {
            GuardedVerify(() => VerifyShaderExists(file, shader),
                e => e is ArgumentException,
                _ =>
                {
                    var shaderPath =
                        FileSystem.Path.GetGameStrippedPath(Repository.Path.AsSpan(), file.FilePath.AsSpan()).ToString();
                    AddError(VerificationError.Create(
                        VerifierChain,
                        VerifierErrorCodes.InvalidShader,
                        $"Invalid texture file name '{shader}' in model '{shaderPath}'",
                        VerificationSeverity.Error,
                        shader, shaderPath));
                });
        }


        foreach (var proxy in file.Content.Proxies)
        {
            GuardedVerify(() => VerifyProxyExists(file, proxy, workingQueue),
                e => e is ArgumentException,
                _ =>
                {
                    var proxyPath = FileSystem.Path
                        .GetGameStrippedPath(Repository.Path.AsSpan(), file.FilePath.AsSpan()).ToString();
                    AddError(VerificationError.Create(
                        VerifierChain,
                        VerifierErrorCodes.InvalidProxy,
                        $"Invalid proxy file name '{proxy}' in model '{proxyPath}'",
                        VerificationSeverity.Error,
                        proxy, proxyPath));
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
            var error = VerificationError.Create(VerifierChain, VerifierErrorCodes.ModelMissingTexture, message, VerificationSeverity.Error, modelFilePath, texture);
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
                VerifierErrorCodes.ModelMissingProxy, message, VerificationSeverity.Error, modelFilePath, proxyName);
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
            var error = VerificationError.Create(VerifierChain, VerifierErrorCodes.ModelMissingShader, message, VerificationSeverity.Error, modelFilePath, shader);
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



public sealed class ReferencedModelsVerifier(
    IGameDatabase database,
    GameVerifySettings settings,
    IServiceProvider serviceProvider)
    : GameVerifierBase(null, database, settings, serviceProvider)
{
    public override string FriendlyName => "Referenced Models";

    public override void Verify(CancellationToken token)
    {
        var models = Database.GameObjectTypeManager.Entries
            .SelectMany(x => x.Models)
            .Concat(FocHardcodedConstants.HardcodedModels);

        var inner = new SharedReferencedModelsVerifier(this, models, Database, Settings, Services);
        try
        {
            inner.Error += OnModelError;
            inner.Verify(token);
        }
        finally
        {
            inner.Error -= OnModelError;
        }
    }

    private void OnModelError(object sender, VerificationErrorEventArgs e)
    {
        AddError(e.Error);
    }
}
