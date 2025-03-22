using AnakinRaW.CommonUtilities.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Engine.Utilities;
using PG.StarWarsGame.Files.ALO.Data;
using PG.StarWarsGame.Files.ALO.Files;
using PG.StarWarsGame.Files.ALO.Files.Animations;
using PG.StarWarsGame.Files.ALO.Services;
using PG.StarWarsGame.Files.Binary;
using System;
using System.IO.Abstractions;
using PG.StarWarsGame.Engine.Rendering.Animations;

namespace PG.StarWarsGame.Engine.Rendering;

internal class PGRender(
    GameRepository gameRepository,
    GameErrorReporterWrapper errorReporter, 
    IServiceProvider serviceProvider) : IPGRender
{
    private readonly IAloFileService _aloFileService = serviceProvider.GetRequiredService<IAloFileService>();
    private readonly IRepository _modelRepository = gameRepository.ModelRepository;
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    private readonly ICrc32HashingService _hashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();
    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(PGRender));

    public IAloFile<IAloDataContent, AloFileInformation>? Load3DAsset(
        string path, 
        bool metadataOnly = true,
        bool throwsException = false)
    {
        return Load3DAsset(path.AsSpan(), metadataOnly, throwsException);
    }

    public IAloFile<IAloDataContent, AloFileInformation>? Load3DAsset(
        ReadOnlySpan<char> path,
        bool metadataOnly = true, 
        bool throwsException = false)
    {
        if (path.IsEmpty)
            errorReporter.Assert(EngineAssert.FromNullOrEmpty(null, "Model path is null or empty."));

        using var aloStream = _modelRepository.TryOpenFile(path);
        if (aloStream is null)
            return null;

        var loadOptions = metadataOnly ? AloLoadOptions.MetadataOnly : AloLoadOptions.Full;

        try
        {
            return _aloFileService.Load(aloStream, loadOptions);
        }
        catch (BinaryCorruptedException e)
        {
            if (throwsException)
                throw;

            var pathString = path.ToString();
            var errorMessage = $"Unable to load 3D asset '{pathString}': {e.Message}";
            _logger?.LogWarning(e, errorMessage);
            errorReporter.Assert(EngineAssert.Create(EngineAssertKind.CorruptBinary, pathString, null, errorMessage));
            return null;
        }
    }

    public ModelClass? LoadModelAndAnimations(
        ReadOnlySpan<char> path, 
        string? animOverrideName, 
        bool metadataOnly = true,
        bool throwsException = false)
    {
        var aloFile = Load3DAsset(path, metadataOnly, throwsException);
        
        if (aloFile is null)
            return null;

        if (!aloFile.FileInformation.IsModel)
            return new ModelClass(aloFile);

        var dirPath = _fileSystem.Path.GetDirectoryName(path);
        var fileName = _fileSystem.Path.GetFileNameWithoutExtension(path);

        if (!string.IsNullOrEmpty(animOverrideName))
            fileName = _fileSystem.Path.GetFileNameWithoutExtension(animOverrideName.AsSpan());

        var animations = LoadAnimations(fileName, dirPath, metadataOnly, throwsException ? AnimationCorruptedHandler : null);

        return new ModelClass(aloFile, animations);
    }

    private void AnimationCorruptedHandler(BinaryCorruptedException e)
    {
        throw e;
    }

    public AnimationCollection LoadAnimations(
        ReadOnlySpan<char> fileName, 
        ReadOnlySpan<char> dirPath, 
        bool metadataOnly = true,
        Action<BinaryCorruptedException>? corruptedAnimationHandler = null)
    {
        fileName = _fileSystem.Path.GetFileNameWithoutExtension(fileName);

        var animations = new AnimationCollection();

        Span<char> stringBuffer = stackalloc char[256];

        foreach (var animationData in SupportedModelAnimationTypes.GetAnimationTypesForEngine(gameRepository.EngineType))
        {
            var subIndex = 0;
            var loadingNumberedAnimations = true;

            var throwsOnLoad = corruptedAnimationHandler is not null;

            while (loadingNumberedAnimations)
            {
                var stringBuilder = new ValueStringBuilder(stringBuffer);

                CreateAnimationFilePath(ref stringBuilder, fileName, animationData.Value, subIndex);
                var animationFilenameWithoutExtension =
                    _fileSystem.Path.GetFileNameWithoutExtension(stringBuilder.AsSpan());
                InsertPath(ref stringBuilder, dirPath);

                if (stringBuilder.Length > PGConstants.MaxAnimationFileName)
                {
                    var animFile = stringBuilder.AsSpan().ToString();
                    errorReporter.Assert(
                        EngineAssert.Create(EngineAssertKind.ValueOutOfRange, animFile, null,
                            $"Cannot get animation file '{animFile}' , because animation file path is too long."));
                    continue;
                }

                try
                {
                    var animationAsset = Load3DAsset(stringBuilder.AsSpan(), metadataOnly, throwsOnLoad);
                    if (animationAsset is IAloAnimationFile animationFile)
                    {
                        loadingNumberedAnimations = true;
                        var crc = _hashingService.GetCrc32(animationFilenameWithoutExtension,
                            PGConstants.DefaultPGEncoding);
                        animations.AddAnimation(animationData.Key, animationFile, crc);
                    }
                    else
                    {
                        loadingNumberedAnimations = false;
                    }
                }
                catch (BinaryCorruptedException e)
                {
                    corruptedAnimationHandler?.Invoke(e);
                    loadingNumberedAnimations = false;
                }
                finally
                {
                    stringBuilder.Dispose();
                    subIndex++;
                }
            }
        }
        return animations;
    }

    private void InsertPath(ref ValueStringBuilder stringBuilder, ReadOnlySpan<char> directory)
    {
        if (!_fileSystem.Path.HasTrailingDirectorySeparator(directory))
            stringBuilder.Insert(0, '\\', 1);
        stringBuilder.Insert(0, directory);
    }

    private static void CreateAnimationFilePath(
        ref ValueStringBuilder stringBuilder,
        ReadOnlySpan<char> fileName, 
        string animationTypeName, 
        int subIndex)
    {
        stringBuilder.Append(fileName);
        stringBuilder.Append('_');
        stringBuilder.Append(animationTypeName);
        stringBuilder.Append('_');
#if NETSTANDARD2_0 || NETFRAMEWORK
        stringBuilder.Append(subIndex.ToString("D2"));
#else
        subIndex.TryFormat(stringBuilder.AppendSpan(2), out var n, "D2");
#endif
        stringBuilder.Append(".ALA");
    }
}