using System;
using System.Collections.Generic;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers.Caching;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Rendering;
using PG.StarWarsGame.Engine.Rendering.Animations;
using PG.StarWarsGame.Files;
using PG.StarWarsGame.Files.ALO.Data;
using PG.StarWarsGame.Files.ALO.Files;
using PG.StarWarsGame.Files.ALO.Files.Animations;
using PG.StarWarsGame.Files.ALO.Files.Models;
using PG.StarWarsGame.Files.ALO.Files.Particles;
using PG.StarWarsGame.Files.Binary;
#if NETFRAMEWORK || NETSTANDARD2_0
using AnakinRaW.CommonUtilities.FileSystem;
#endif

namespace AET.ModVerify.Verifiers.Commons;

public sealed class SingleModelVerifier : GameVerifierBase
{
    private readonly TextureVerifier _textureVerifier;
    private readonly IAlreadyVerifiedCache? _cache;

    private bool _textureVerifierSubscribed;

    public SingleModelVerifier(GameVerifierBase parent) : base(parent)
    {
        _textureVerifier = new TextureVerifier(this);
        _cache = Services.GetService<IAlreadyVerifiedCache>();
    }

    public SingleModelVerifier(
        IGameVerifierInfo? parent,
        IStarWarsGameEngine engine,
        GameVerifySettings settings,
        IServiceProvider serviceProvider) : base(parent, engine, settings, serviceProvider)
    {
        _textureVerifier = new TextureVerifier(this);
        _cache = serviceProvider.GetService<IAlreadyVerifiedCache>();
    }

    public ModelClass? VerifyAlamoFile(string fileName, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        return VerifyWithCache(fileName, contextInfo,
            _ => AddNotExistError(fileName, contextInfo),
            alamoObject =>
            {
                VerifyModelClass(alamoObject, contextInfo, token);
                return alamoObject;
            },
            token);
    }

    public ModelClass? VerifyModel(string fileName, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        return VerifyWithCache(fileName, contextInfo, 
            _ => AddNotExistError(fileName, contextInfo),
            alamoObject => VerifyType<AlamoModel>(alamoObject, contextInfo, token),
            token);
    }

    public ModelClass? VerifyParticle(string fileName, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        return VerifyWithCache(fileName, contextInfo, 
            _ => AddNotExistError(fileName, contextInfo),
            alamoObject => VerifyType<AlamoParticle>(alamoObject, contextInfo, token),
            token);
    }

    public ModelClass? VerifyAnimation(string fileName, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        return VerifyWithCache(fileName, contextInfo,
            _ => AddNotExistError(fileName, contextInfo),
            alamoObject => VerifyType<AlamoAnimation>(alamoObject, contextInfo, token),
            token);
    }
    
    private ModelClass? VerifyWithCache(
        string fileName,
        IReadOnlyCollection<string> contextInfo, 
        Action<string> notExistsAction,
        Func<ModelClass, ModelClass?> verifyObject,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        
        var cacheEntry = _cache?.GetEntry(fileName);
        if (cacheEntry?.AlreadyVerified is true)
        {
            if (!cacheEntry.Value.AssetExists) 
                notExistsAction(fileName);
            return null;
        }

        var modelClass = LoadModel(fileName, contextInfo, out var exists);

        _cache?.TryAddEntry(fileName, exists);

        if (!exists)
            notExistsAction(fileName);

        if (modelClass is null)
            return null;


        var isSubscriber = false;
        try
        {
            
            if (!_textureVerifierSubscribed)
            {
                _textureVerifier.Error += OnTextureError;
                isSubscriber = true;
                _textureVerifierSubscribed = true;
            }
            return verifyObject(modelClass);
        }
        finally
        {
            if (isSubscriber && _textureVerifierSubscribed)
            {
                _textureVerifier.Error -= OnTextureError;
                _textureVerifierSubscribed = false;
            }
        }
    }

    private ModelClass? VerifyType<T>(
        ModelClass modelClass,
        IReadOnlyCollection<string> contextInfo,
        CancellationToken token)
        where T : IAloDataContent
    {
        if (modelClass.RenderableContent is not T)
        {
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes.UnexpectedBinaryFormat,
                $"Expected Alamo object of type {typeof(T).Name}, but got {modelClass.RenderableContent.GetType().Name}",
                VerificationSeverity.Error,
                contextInfo,
                NormalizeFileName(modelClass.File.FileName)));
            return null;
        }

        VerifyModelClass(modelClass, contextInfo, token);
        return modelClass;
    }

    private void VerifyModelClass(ModelClass modelClass, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        switch (modelClass.File)
        {
            case IAloModelFile model:
                VerifyModel(model, modelClass.Animations, contextInfo, token);
                return;
            case IAloParticleFile particle:
                VerifyParticle(particle, contextInfo);
                return;
            case IAloAnimationFile animation:
                VerifyAnimation(animation, contextInfo, token);
                return;
            default:
                throw new InvalidOperationException("Unsupported Alamo file type.");
        }
    }
    
    private ModelClass? LoadModel(string fileName, IReadOnlyCollection<string> contextInfo, out bool exists)
    {
        IAloFile<IAloDataContent, AloFileInformation>? alamoFile;
        
        var modelPath = FileSystem.Path.Combine("DATA\\ART\\MODELS", fileName);
        
        try
        {
            alamoFile = GameEngine.PGRender.Load3DAsset(modelPath, true, true);
        }
        catch (BinaryCorruptedException e)
        {
            var message = $"'{fileName}' is corrupted: {e.Message}";
            AddError(VerificationError.Create(
                this, 
                VerifierErrorCodes.BinaryFileCorrupt,
                message,
                VerificationSeverity.Critical, 
                contextInfo, 
                NormalizeFileName(fileName)));
            exists = true;
            return null;
        }

        // Because throwsException is true, we know that if aloFile is null,
        // the file does not exist
        exists = alamoFile is not null;

        if (alamoFile is null)
        {
            exists = false;
            return null;
        }

        exists = true;

        var animationCollection = AnimationCollection.Empty;
        if (alamoFile.Content is AlamoModel)
        { 
            animationCollection = GameEngine.PGRender.LoadAnimations(
                alamoFile.FileName, @"DATA\ART\MODELS", true,
                (_, _, alaFile) =>      
                {
                    var alaFileName = NormalizeFileName(alaFile);
                    AddError(VerificationError.Create(
                        this,
                        VerifierErrorCodes.BinaryFileCorrupt,
                        $"Invalid animation file '{alaFileName}' for model '{alamoFile.FileName}'",
                        VerificationSeverity.Error,
                        [NormalizeFileName(alamoFile.FileName)],
                        alaFileName));
                });
        }

        return new ModelClass(alamoFile, animationCollection);
    }

    private void VerifyParticle(IAloParticleFile file, IReadOnlyCollection<string> contextInfo)
    {
        IReadOnlyList<string> particleContext = [.. contextInfo, NormalizeFileName(file.FileName)];
            
        foreach (var texture in file.Content.Textures)
        {
            GuardedVerify(() => VerifyTextureExists(file, texture, particleContext),
                e => e is ArgumentException,
                _ =>
                {
                    AddError(VerificationError.Create(
                        this,
                        VerifierErrorCodes.InvalidFilePath,
                        $"Invalid texture file name '{texture}' in particle '{file.FileName}'",
                        VerificationSeverity.Error,
                        particleContext, 
                        texture));
                });
        }

        var fileName = FileSystem.Path.GetFileNameWithoutExtension(file.FileName.AsSpan());
        var name = file.Content.Name.AsSpan();

        if (!fileName.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes.InvalidParticleName,
                $"The particle name '{file.Content.Name}' does not match file name '{file.FileName}'",
                VerificationSeverity.Error,
                particleContext,
                file.Content.Name));
        }

    }

    private void VerifyModel(IAloModelFile file, AnimationCollection animations, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        IReadOnlyList<string> modelContext = [.. contextInfo, NormalizeFileName(file.FileName)];

        foreach (var texture in file.Content.Textures)
        {
            GuardedVerify(() => VerifyTextureExists(file, texture, modelContext),
                e => e is ArgumentException,
                _ =>
                {
                    AddError(VerificationError.Create(
                        this,
                        VerifierErrorCodes.InvalidFilePath,
                        $"Invalid texture file name '{texture}' in model '{file.FileName}'",
                        VerificationSeverity.Error,
                        modelContext, 
                        texture));
                });
        }

        foreach (var shader in file.Content.Shaders)
        {
            GuardedVerify(() => VerifyShaderExists(file, shader, contextInfo),
                e => e is ArgumentException,
                _ =>
                {
                    AddError(VerificationError.Create(
                        this,
                        VerifierErrorCodes.InvalidFilePath,
                        $"Invalid shader file name '{shader}' in model '{file.FileName}'",
                        VerificationSeverity.Error,
                        modelContext,
                        shader));
                });
        }

        foreach (var proxy in file.Content.Proxies)
        {
            GuardedVerify(() => VerifyProxyExists(file, proxy, modelContext, token),
                e => e is ArgumentException,
                _ =>
                {
                    AddError(VerificationError.Create(
                        this,
                        VerifierErrorCodes.InvalidFilePath,
                        $"Invalid proxy file name '{proxy}' for model '{file.FileName}'",
                        VerificationSeverity.Error,
                        modelContext,
                        proxy));
                });
        }

        foreach (var animation in animations) 
            VerifyAnimationOfModel(animation, file.Content, modelContext, token);
    }

    private void VerifyAnimationOfModel(IAloAnimationFile file, AlamoModel model, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        VerifyAnimation(file, contextInfo, token);
        // TODO - Verify that the animation is using correct bones for the model, and that it doesn't use any bones that the model doesn't have
    }

    private void VerifyAnimation(IAloAnimationFile file, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        // TODO
        // Is there actually anything to verify for animation without looking at the model?
    }

    private void VerifyTextureExists(IPetroglyphFileHolder file, string texture, IReadOnlyCollection<string> contextInfo)
    {
        if (string.IsNullOrEmpty(texture))
        {
            AddError(VerificationError.Create(this, 
                VerifierErrorCodes.InvalidValue,
                $"Texture string in model or particle '{file.FileName}' is empty.'",
                VerificationSeverity.Error,
                contextInfo,
                NormalizeFileName(file.FileName)));
            return;
        }
        if (texture == "None")
            return;
        _textureVerifier.Verify(texture, contextInfo, CancellationToken.None);
    }

    private void VerifyProxyExists(IPetroglyphFileHolder model, string proxy, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        var proxyName = ModelClass.GetProxyName(proxy).ToString();

        if (string.IsNullOrEmpty(proxyName))
        {
            AddError(VerificationError.Create(this, 
                VerifierErrorCodes.InvalidValue,
                $"Proxy name in model '{model.FileName}' is empty.'",
                VerificationSeverity.Error,
                contextInfo,
                NormalizeFileName(model.FileName)));
            return;
        }
        
        VerifyWithCache(proxyName, contextInfo, _ =>
        {
            var message = $"Proxy particle '{proxyName}' not found for model '{model.FileName}'";
            var error = VerificationError.Create(
                this,
                VerifierErrorCodes.FileNotFound,
                message,
                VerificationSeverity.Error,
                contextInfo,
                proxyName);
            AddError(error);
        }, proxyObject => VerifyType<AlamoParticle>(proxyObject, contextInfo, token), token);
    }

    private void VerifyShaderExists(IPetroglyphFileHolder model, string shader, IReadOnlyCollection<string> contextInfo)
    {
        if (shader is "alDefault.fx" or "alDefault.fxo")
            return;

        if (!Repository.EffectsRepository.FileExists(shader))
        {
            var message = $"Shader effect '{shader}' not found for model '{model.FileName}'.";
            var error = VerificationError.Create(
                this,
                VerifierErrorCodes.FileNotFound, 
                message, 
                VerificationSeverity.Error, 
                [..contextInfo, NormalizeFileName(model.FileName)], 
                shader);
            AddError(error);
        }
    }

    private string NormalizeFileName(string fileName)
    {
        return GameEngine.GameRepository.PGFileSystem.GetFileName(fileName).ToUpperInvariant();
    }

    private void AddNotExistError(string fileName, IReadOnlyCollection<string> contextInfo)
    {
        AddError(VerificationError.Create(
            this,
            VerifierErrorCodes.FileNotFound,
            $"Unable to find Alamo file '{fileName}'",
            VerificationSeverity.Error,
            contextInfo,
            NormalizeFileName(fileName)));
    }

    private void OnTextureError(object sender, VerificationErrorEventArgs e)
    {
        AddError(e.Error);
    }
}