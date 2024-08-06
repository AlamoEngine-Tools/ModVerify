using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons.Binary;
using PG.Commons.Files;
using PG.Commons.Utilities;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Files.ALO.Files.Models;
using PG.StarWarsGame.Files.ALO.Files.Particles;
using PG.StarWarsGame.Files.ALO.Services;
using PG.StarWarsGame.Files.ChunkFiles.Data;
using AnakinRaW.CommonUtilities.FileSystem;

namespace AET.ModVerify.Verifiers;

public sealed class ReferencedModelsVerifier(
    IGameDatabase database,
    GameVerifySettings settings,
    IServiceProvider serviceProvider)
    : GameVerifierBase(database, settings, serviceProvider)
{
    private const string ProxyAltIdentifier = "_ALT";

    private readonly IAloFileService _modelFileService = serviceProvider.GetRequiredService<IAloFileService>();

    public override string FriendlyName => "Referenced Models";

    protected override void RunVerification(CancellationToken token)
    {
        var aloQueue = new Queue<string>(Database.GameObjects.Entries
            .SelectMany(x => x.Models)
            .Concat(FocHardcodedConstants.HardcodedModels));
        
        var visitedAloFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (aloQueue.Count != 0)
        {
            var model = aloQueue.Dequeue();
            if (!visitedAloFiles.Add(model))
                continue;

            token.ThrowIfCancellationRequested();

            using var modelStream = Repository.TryOpenFile(BuildModelPath(model));

            if (modelStream is null)
            {
                var error = VerificationError.Create(
                    this,
                    VerifierErrorCodes.ModelNotFound, 
                    $"Unable to find .ALO file '{model}'", 
                    VerificationSeverity.Error, 
                    model);

                AddError(error);
            }
            else
                VerifyModelOrParticle(modelStream, aloQueue);
        }
    }

    private void VerifyModelOrParticle(Stream modelStream, Queue<string> workingQueue)
    {
        try
        {
            using var aloData = _modelFileService.Load(modelStream, AloLoadOptions.Assets);
            switch (aloData)
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
        catch (BinaryCorruptedException e)
        {
            var aloFile = GetGameStrippedPath(modelStream.GetFilePath());
            var message = $"{aloFile} is corrupted: {e.Message}";
            AddError(VerificationError.Create(this, VerifierErrorCodes.ModelBroken, message, VerificationSeverity.Critical, aloFile));
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
                    var modelFilePath = GetGameStrippedPath(file.FilePath);
                    AddError(VerificationError.Create(
                        this,
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
            var modelFilePath = GetGameStrippedPath(file.FilePath);
            AddError(VerificationError.Create(
                this,
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
                    var modelFilePath = GetGameStrippedPath(file.FilePath);
                    AddError(VerificationError.Create(
                        this,
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
                    var modelFilePath = GetGameStrippedPath(file.FilePath);
                    AddError(VerificationError.Create(
                        this,
                        VerifierErrorCodes.InvalidShader,
                        $"Invalid texture file name '{shader}' in model '{modelFilePath}'",
                        VerificationSeverity.Error,
                        shader, modelFilePath));
                });
        }


        foreach (var proxy in file.Content.Proxies)
        {
            GuardedVerify(() => VerifyProxyExists(file, proxy, workingQueue),
                e => e is ArgumentException,
                _ =>
                {
                    var modelFilePath = GetGameStrippedPath(file.FilePath);
                    AddError(VerificationError.Create(
                        this,
                        VerifierErrorCodes.InvalidProxy,
                        $"Invalid proxy file name '{proxy}' in model '{modelFilePath}'",
                        VerificationSeverity.Error,
                        proxy, modelFilePath));
                });
        }
    }

    private void VerifyTextureExists(IPetroglyphFileHolder<IChunkData, PetroglyphFileInformation> model, string texture)
    {
        if (texture == "None")
            return;
        
        if (!Repository.TextureRepository.FileExists(texture))
        {
            var modelFilePath = GetGameStrippedPath(model.FilePath);
            var message = $"{modelFilePath} references missing texture: {texture}";
            var error = VerificationError.Create(this, VerifierErrorCodes.ModelMissingTexture, message, VerificationSeverity.Error, modelFilePath, texture);
            AddError(error);
        }
    }

    private void VerifyProxyExists(IPetroglyphFileHolder model, string proxy, Queue<string> workingQueue)
    {
        var proxyName = ProxyNameWithoutAlt(proxy);
        var particle = FileSystem.Path.ChangeExtension(proxyName, "alo");
        if (!Repository.FileExists(BuildModelPath(particle)))
        {
            var modelFilePath = GetGameStrippedPath(model.FilePath);
            var message = $"{modelFilePath} references missing proxy particle: {particle}";
            var error = VerificationError.Create(this, VerifierErrorCodes.ModelMissingProxy, message, VerificationSeverity.Error, modelFilePath, particle);
            AddError(error);
        }
        else
            workingQueue.Enqueue(particle);
    }

    private string BuildModelPath(string fileName)
    {
        return FileSystem.Path.Combine("DATA/ART/MODELS", fileName);
    }

    private void VerifyShaderExists(IPetroglyphFileHolder data, string shader)
    {
        if (shader is "alDefault.fx" or "alDefault.fxo")
            return;

        if (!Repository.EffectsRepository.FileExists(shader))
        {
            var modelFilePath = GetGameStrippedPath(data.FilePath);
            var message = $"{modelFilePath} references missing shader effect: {shader}";
            var error = VerificationError.Create(this, VerifierErrorCodes.ModelMissingShader, message, VerificationSeverity.Error, modelFilePath, shader);
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
