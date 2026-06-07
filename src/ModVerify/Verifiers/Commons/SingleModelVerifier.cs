using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Diagnostics = AET.ModVerify.Reporting.Diagnostics;
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
            AddError(Diagnostics.Models.UnexpectedAloType(this, typeof(T).Name,
                modelClass.RenderableContent.GetType().Name, NormalizeFileName(modelClass.File.FileName), contextInfo));
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
            if (!CheckBinaryCorruptedFileIsActuallyRenderable(fileName, out var actualFilePath))
            {
                // Error, because loading a model/particle directly impacts game behavior and would be very hard to debug
                // for mod creators, unaware of the CRC32 collision issue.
                AddError(Diagnostics.Models.CrcCollision(this, NormalizeFileName(fileName), actualFilePath, contextInfo));
            }
            else
            {
                AddError(Diagnostics.Models.CorruptModel(this, NormalizeFileName(fileName), e.Message, contextInfo));
            }

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

                   if (!CheckBinaryCorruptedFileIsActuallyRenderable(alaFileName, out var actualFilePath))
                   {
                       // Information, because for animations, as there is more likely to be a CRC32 collision than an actual corrupted file.
                       // This is because the engine attempts to load all possible animations for each model and thus
                       // there are simply more chances for a CRC32 collision.
                       AddError(Diagnostics.Models.AnimationCrcCollision(this, NormalizeFileName(alaFileName), actualFilePath, contextInfo));
                   }
                   else
                   {
                       AddError(Diagnostics.Models.CorruptAnimation(this, alaFileName, alamoFile.FileName, [NormalizeFileName(alamoFile.FileName)]));
                   }
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
                    AddError(Diagnostics.Models.InvalidParticleTextureName(this, texture, file.FileName, particleContext));
                });
        }

        var fileName = FileSystem.Path.GetFileNameWithoutExtension(file.FileName.AsSpan());
        var name = file.Content.Name.AsSpan();

        if (!fileName.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            AddError(Diagnostics.Models.ParticleNameMismatch(this, file.Content.Name, file.FileName, particleContext));
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
                    AddError(Diagnostics.Models.InvalidModelTextureName(this, texture, file.FileName, modelContext));
                });
        }

        foreach (var shader in file.Content.Shaders)
        {
            GuardedVerify(() => VerifyShaderExists(file, shader, contextInfo),
                e => e is ArgumentException,
                _ =>
                {
                    AddError(Diagnostics.Models.InvalidShaderName(this, shader, file.FileName, modelContext));
                });
        }

        foreach (var proxy in file.Content.Proxies)
        {
            GuardedVerify(() => VerifyProxyExists(file, proxy, modelContext, token),
                e => e is ArgumentException,
                _ =>
                {
                    AddError(Diagnostics.Models.InvalidProxyName(this, proxy, file.FileName, modelContext));
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
            AddError(Diagnostics.Models.EmptyTextureName(this, NormalizeFileName(file.FileName), contextInfo));
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
            AddError(Diagnostics.Models.EmptyProxyName(this, NormalizeFileName(model.FileName), contextInfo));
            return;
        }
        
        VerifyWithCache(proxyName, contextInfo,
            _ => AddError(Diagnostics.Models.ProxyNotFound(this, proxyName, model.FileName, contextInfo)),
            proxyObject => VerifyType<AlamoParticle>(proxyObject, contextInfo, token), token);
    }

    private void VerifyShaderExists(IPetroglyphFileHolder model, string shader, IReadOnlyCollection<string> contextInfo)
    {
        if (shader is "alDefault.fx" or "alDefault.fxo")
            return;

        if (!Repository.EffectsRepository.FileExists(shader))
        {
            AddError(Diagnostics.Models.ShaderNotFound(this, shader, model.FileName, [..contextInfo, NormalizeFileName(model.FileName)]));
        }
    }

    // NB: This method assures that the BinaryCorruptedException resulted from a file
    // that is actually an Alamo file (and thus should be reported as a corrupted file),
    // and not from some other file that was found due to e.g., CRC32 collision.
    private bool CheckBinaryCorruptedFileIsActuallyRenderable(string fileName, out string actualFilePath)
    {
        var filePath = FileSystem.Path.Join(@"DATA\ART\MODELS", fileName);
        var exists = GameEngine.GameRepository.ModelRepository.FileExists(filePath, false, out _, out actualFilePath!);
        Debug.Assert(exists);

        var extension = FileSystem.Path.GetExtension(actualFilePath);

        return string.IsNullOrEmpty(actualFilePath) || extension.Equals(".alo", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".ala", StringComparison.OrdinalIgnoreCase);
    }

    private string NormalizeFileName(string fileName)
    {
        return GameEngine.GameRepository.PGFileSystem.GetFileName(fileName).ToUpperInvariant();
    }

    private void AddNotExistError(string fileName, IReadOnlyCollection<string> contextInfo)
    {
        AddError(Diagnostics.Models.AlamoFileNotFound(this, NormalizeFileName(fileName), contextInfo));
    }

    private void OnTextureError(object sender, VerificationErrorEventArgs e)
    {
        AddError(e.Error);
    }
}