using System;
using System.Collections.Generic;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AET.ModVerify.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Files;
using PG.StarWarsGame.Files.ALO.Data;
using PG.StarWarsGame.Files.ALO.Files;
using PG.StarWarsGame.Files.ALO.Files.Models;
using PG.StarWarsGame.Files.ALO.Files.Particles;
using PG.StarWarsGame.Files.Binary;
#if NETSTANDARD2_0 || NETFRAMEWORK
using AnakinRaW.CommonUtilities.FileSystem;
#endif

namespace AET.ModVerify.Verifiers.Commons;

public sealed class SingleModelVerifier : GameVerifier<string>
{
    private const string ProxyAltIdentifier = "_ALT";

    private readonly TextureVeifier _textureVerifier;
    private readonly IAlreadyVerifiedCache? _cache;

    public SingleModelVerifier(IGameVerifierInfo? parent,
        IGameDatabase database,
        GameVerifySettings settings,
        IServiceProvider serviceProvider) : base(parent, database, settings, serviceProvider)
    {
        _textureVerifier = new TextureVeifier(this, database, settings, serviceProvider);
        _cache = serviceProvider.GetService<IAlreadyVerifiedCache>();
    }

    public override void Verify(string modelName, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        try
        {
            _textureVerifier.Error += OnTextureError;

            var modelPath = BuildModelPath(modelName);
            VerifyAlamoFile(modelPath, contextInfo, token);
        }
        finally
        {
            _textureVerifier.Error -= OnTextureError;
        }
    }

    private void OnTextureError(object sender, VerificationErrorEventArgs e)
    {
        AddError(e.Error);
    }

    private void VerifyAlamoFile(string modelPath, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var modelName = FileSystem.Path.GetFileName(modelPath.AsSpan());

        if (_cache?.TryAddEntry(modelName) == false)
            return;
        
        IAloFile<IAloDataContent, AloFileInformation>? aloFile = null;
        try
        {
            try
            {
                aloFile = Database.PGRender.Load3DAsset(modelPath, true, true);
            }
            catch (BinaryCorruptedException e)
            {
                var aloFilePath = FileSystem.Path.GetGameStrippedPath(Repository.Path.AsSpan(), modelPath.AsSpan()).ToString();
                var message = $"'{aloFilePath}' is corrupted: {e.Message}";
                AddError(VerificationError.Create(VerifierChain, VerifierErrorCodes.FileCorrupt, message,
                    VerificationSeverity.Critical, contextInfo, aloFilePath));
                return;
            }

            if (aloFile is null)
            {
                var modelNameString = modelName.ToString();
                var error = VerificationError.Create(
                    VerifierChain,
                    VerifierErrorCodes.FileNotFound,
                    $"Unable to find .ALO file '{modelNameString}'",
                    VerificationSeverity.Error,
                    contextInfo,
                    modelNameString);
                AddError(error);
                return;
            }

            VerifyModelOrParticle(aloFile, contextInfo, token);
        }
        finally
        {
            aloFile?.Dispose();
        }
    }

    private void VerifyModelOrParticle(
        IAloFile<IAloDataContent, AloFileInformation> aloFile, 
        IReadOnlyCollection<string> contextInfo,
        CancellationToken token)
    {
        switch (aloFile)
        {
            case IAloModelFile model:
                VerifyModel(model, contextInfo, token);
                break;
            case IAloParticleFile particle:
                VerifyParticle(particle, contextInfo);
                break;
            default:
                throw new InvalidOperationException("The data stream is neither a model nor particle.");
        }
    }

    private void VerifyParticle(IAloParticleFile file, IReadOnlyCollection<string> contextInfo)
    {
        foreach (var texture in file.Content.Textures)
        {
            GuardedVerify(() => VerifyTextureExists(file, texture, contextInfo),
                e => e is ArgumentException,
                _ =>
                {
                    var particlePath = FileSystem.Path.GetGameStrippedPath(Repository.Path.AsSpan(), file.FilePath.AsSpan()).ToString();
                    AddError(VerificationError.Create(
                        VerifierChain,
                        VerifierErrorCodes.InvalidFilePath,
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

    private void VerifyModel(IAloModelFile file, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        foreach (var texture in file.Content.Textures)
        {
            GuardedVerify(() => VerifyTextureExists(file, texture, contextInfo),
                e => e is ArgumentException,
                _ =>
                {
                    var modelFilePath = FileSystem.Path.GetGameStrippedPath(Repository.Path.AsSpan(), file.FilePath.AsSpan()).ToString();
                    AddError(VerificationError.Create(
                        VerifierChain,
                        VerifierErrorCodes.InvalidFilePath,
                        $"Invalid texture file name '{texture}' in model '{modelFilePath}'",
                        VerificationSeverity.Error,
                        [modelFilePath], 
                        texture));
                });
        }

        foreach (var shader in file.Content.Shaders)
        {
            GuardedVerify(() => VerifyShaderExists(file, shader, contextInfo),
                e => e is ArgumentException,
                _ =>
                {
                    var modelFilePath =
                        FileSystem.Path.GetGameStrippedPath(Repository.Path.AsSpan(), file.FilePath.AsSpan()).ToString();
                    AddError(VerificationError.Create(
                        VerifierChain,
                        VerifierErrorCodes.InvalidFilePath,
                        $"Invalid shader file name '{shader}' in model '{modelFilePath}'",
                        VerificationSeverity.Error,
                        [modelFilePath],
                        shader));
                });
        }


        foreach (var proxy in file.Content.Proxies)
        {
            GuardedVerify(() => VerifyProxyExists(file, proxy, contextInfo, token),
                e => e is ArgumentException,
                _ =>
                {
                    var modelFilePath = FileSystem.Path
                        .GetGameStrippedPath(Repository.Path.AsSpan(), file.FilePath.AsSpan()).ToString();
                    AddError(VerificationError.Create(
                        VerifierChain,
                        VerifierErrorCodes.InvalidFilePath,
                        $"Invalid proxy file name '{proxy}' for model '{modelFilePath}'",
                        VerificationSeverity.Error,
                        [..contextInfo, modelFilePath],
                        proxy));
                });
        }
    }

    private void VerifyTextureExists(IPetroglyphFileHolder model, string texture, IReadOnlyCollection<string> contextInfo)
    {
        if (texture == "None")
            return;
        _textureVerifier.Verify(texture, [..contextInfo, model.FileName], CancellationToken.None);
    }

    private void VerifyProxyExists(IPetroglyphFileHolder model, string proxy, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        var proxyName = ProxyNameWithoutAlt(proxy);
        var proxyPath = BuildModelPath(proxyName);

        var modelFilePath = FileSystem.Path.GetGameStrippedPath(Repository.Path.AsSpan(), model.FilePath.AsSpan()).ToString();

        if (!Repository.ModelRepository.FileExists(proxyPath))
        {
            var message = $"Proxy particle '{proxyName}' not found for model '{modelFilePath}'";
            var error = VerificationError.Create(
                VerifierChain,
                VerifierErrorCodes.FileNotFound,
                message, 
                VerificationSeverity.Error, 
                [..contextInfo, modelFilePath], 
                proxyName);
            AddError(error);
            return;
        }
        
        VerifyAlamoFile(proxyPath, [..contextInfo, modelFilePath], token);
    }

    private void VerifyShaderExists(IPetroglyphFileHolder model, string shader, IReadOnlyCollection<string> contextInfo)
    {
        if (shader is "alDefault.fx" or "alDefault.fxo")
            return;

        if (!Repository.EffectsRepository.FileExists(shader))
        {
            var modelFilePath = FileSystem.Path.GetGameStrippedPath(Repository.Path.AsSpan(), model.FilePath.AsSpan()).ToString();
            var message = $"Shader effect '{shader}' not found for model '{modelFilePath}'.";
            var error = VerificationError.Create(
                VerifierChain, 
                VerifierErrorCodes.FileNotFound, 
                message, 
                VerificationSeverity.Error, 
                [..contextInfo, modelFilePath], 
                shader);
            AddError(error);
        }
    }

    private string BuildModelPath(string fileName)
    {
        return FileSystem.Path.Combine("DATA\\ART\\MODELS", fileName);
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