using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons.Services;
using PG.Commons.Utilities;
using PG.StarWarsGame.Files.ALO.Binary;
using PG.StarWarsGame.Files.ALO.Binary.Identifier;
using PG.StarWarsGame.Files.ALO.Data;
using PG.StarWarsGame.Files.ALO.Files;
using PG.StarWarsGame.Files.ALO.Files.Animations;
using PG.StarWarsGame.Files.ALO.Files.Models;
using PG.StarWarsGame.Files.ALO.Files.Particles;

namespace PG.StarWarsGame.Files.ALO.Services;

public class AloFileService(IServiceProvider serviceProvider) : ServiceBase(serviceProvider), IAloFileService
{
    private readonly IAloFileReaderFactory _readerFactory = serviceProvider.GetRequiredService<IAloFileReaderFactory>();
    private readonly IAloContentInfoIdentifier _aloContentIdentifier = serviceProvider.GetRequiredService<IAloContentInfoIdentifier>();

    public IAloFile<IAloDataContent, AloFileInformation> Load(Stream stream, AloLoadOptions loadOptions = AloLoadOptions.Full)
    {
        return Load(stream, null, loadOptions);
    }

    public IAloModelFile LoadModel(Stream stream, AloLoadOptions loadOptions = AloLoadOptions.Full)
    {
        return (IAloModelFile)Load(stream, AloType.Model, loadOptions);
    }

    public IAloParticleFile LoadParticle(Stream stream, AloLoadOptions loadOptions = AloLoadOptions.Full)
    {
        return (IAloParticleFile)Load(stream, AloType.Particle, loadOptions);
    }

    public IAloAnimationFile LoadAnimation(Stream stream, AloLoadOptions loadOptions = AloLoadOptions.Full)
    {
        return (IAloAnimationFile)Load(stream, AloType.Animation, loadOptions);
    }

    public IAloFile<IAloDataContent, AloFileInformation> Load(Stream stream, AloType? supportedType = null,
        AloLoadOptions loadOptions = AloLoadOptions.Full)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        var startPosition = stream.Position;

        var contentInfo = _aloContentIdentifier.GetContentInfo(stream);
        if (supportedType is not null)
        {
            if (supportedType != contentInfo.Type)
                throw new NotSupportedException($"ALO content was expected to be {supportedType} but was {contentInfo.Type}");
        }

        // Reset Stream
        stream.Seek(startPosition, SeekOrigin.Begin);

        using var reader = _readerFactory.GetReader(contentInfo, stream, loadOptions);

        var alo = reader.Read();

        var filePath = stream.GetFilePath(out var isInMeg);
        var fileInfo = new AloFileInformation(filePath, isInMeg, contentInfo);

        if (alo is AlamoModel model)
            return new AloModelFile(model, fileInfo, Services);

        if (alo is AlamoParticle particle)
            return new AloParticleFile(particle, fileInfo, Services);

        if (alo is AlamoAnimation animation)
            return new AloAnimationFile(animation, fileInfo, Services);

        throw new InvalidOperationException();
    }
    
    public AloContentInfo GetAloContentInfo(Stream stream)
    {
        return _aloContentIdentifier.GetContentInfo(stream);
    }
}