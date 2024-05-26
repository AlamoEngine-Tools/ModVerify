using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons.Binary;
using PG.Commons.Files;
using PG.Commons.Utilities;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.FileSystem;
using PG.StarWarsGame.Engine.Pipeline;
using PG.StarWarsGame.Files.ALO.Files.Models;
using PG.StarWarsGame.Files.ALO.Files.Particles;
using PG.StarWarsGame.Files.ALO.Services;
using PG.StarWarsGame.Files.ChunkFiles.Data;
namespace RepublicAtWar.DevLauncher.Petroglyph.Verification.Steps;

internal class VerifyReferencedModelsStep(CreateGameDatabaseStep createDatabaseStep, IGameRepository repository, IServiceProvider serviceProvider)
    : GameVerificationStep(createDatabaseStep, repository, serviceProvider)
{
    public const string ModelNotFound = "ALO00";
    public const string ModelBroken = "ALO01";
    public const string ModelMissingTexture = "ALO02";
    public const string ModelMissingProxy = "ALO03";
    public const string ModelMissingShader = "ALO04";

    private const string ProxyAltIdentifier = "_ALT";

    private static readonly string[] TextureExtensions = ["dds", "tga"];

    private readonly IAloFileService _modelFileService = serviceProvider.GetRequiredService<IAloFileService>();

    protected override string LogFileName => "ModelTextureShader";

    protected override void RunVerification(CancellationToken token)
    {
        var aloQueue = new Queue<string>(Database.GameObjects
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
                var error = VerificationError<string>.Create(ModelNotFound, $"Unable to find .ALO data: {model}", model);
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
            var aloFile = modelStream.GetFilePath();
            var message = $"{aloFile} is corrupted: {e.Message}";
            AddError(VerificationError<string>.Create(ModelBroken, message, aloFile));
        }
    }

    private void VerifyParticle(IAloParticleFile particle)
    {
    }

    private void VerifyModel(IAloModelFile file, Queue<string> workingQueue)
    {
        foreach (var texture in file.Content.Textures)
        {
            GuardedVerify(() => VerifyTextureExists(file, texture),
                e => e is ArgumentException,
                $"texture '{texture}'");
        }

        foreach (var shader in file.Content.Shaders)
        {
            GuardedVerify(() => VerifyShaderExists(file, shader),
                e => e is ArgumentException,
                $"shader '{shader}'");
        }


        foreach (var proxy in file.Content.Proxies)
        {
            GuardedVerify(() => VerifyProxyExists(file, proxy, workingQueue),
                e => e is ArgumentException,
                $"proxy '{proxy}'");
        }
    }

    private void VerifyTextureExists(IPetroglyphFileHolder<IChunkData, PetroglyphFileInformation> model, string texture)
    {
        if (texture == "None")
            return;
        var texturePath = FileSystem.Path.Combine("Data/Art/Textures", texture);
        if (!Repository.FileExists(texturePath, TextureExtensions))
        {
            var message = $"{model.FilePath} references missing texture: {texture}";
            var error = VerificationError<(string Model, string Texture)>
                .Create(ModelMissingTexture, message, (model.FilePath, texture));
            AddError(error);
        }
    }

    private void VerifyProxyExists(IPetroglyphFileHolder model, string proxy, Queue<string> workingQueue)
    {
        var proxyName = ProxyNameWithoutAlt(proxy);
        var particle = FileSystem.Path.ChangeExtension(proxyName, "alo");
        if (!Repository.FileExists(BuildModelPath(particle)))
        {
            var message = $"{model.FilePath} references missing proxy particle: {particle}";
            var error = VerificationError<(string Model, string Proxy)>
                .Create(ModelMissingProxy, message, (model.FilePath, particle));
            AddError(error);
        }
        else
            workingQueue.Enqueue(particle);
    }

    private string BuildModelPath(string fileName)
    {
        return FileSystem.Path.Combine("Data/Art/Models", fileName);
    }

    private void VerifyShaderExists(IPetroglyphFileHolder data, string shader)
    {
        if (shader is "alDefault.fx" or "alDefault.fxo")
            return;

        if (!Repository.EffectsRepository.FileExists(shader))
        {
            var message = $"{data.FilePath} references missing shader effect: {shader}";
            var error = VerificationError<(string, string)>
                .Create(ModelMissingShader, message, (data.FilePath, shader));
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
