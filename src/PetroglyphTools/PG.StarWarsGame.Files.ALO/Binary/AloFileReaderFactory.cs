using System;
using System.IO;
using PG.StarWarsGame.Files.ALO.Binary.Reader;
using PG.StarWarsGame.Files.ALO.Binary.Reader.Animations;
using PG.StarWarsGame.Files.ALO.Binary.Reader.Models;
using PG.StarWarsGame.Files.ALO.Binary.Reader.Particles;
using PG.StarWarsGame.Files.ALO.Data;
using PG.StarWarsGame.Files.ALO.Files;
using PG.StarWarsGame.Files.ALO.Services;

namespace PG.StarWarsGame.Files.ALO.Binary;

internal class AloFileReaderFactory(IServiceProvider serviceProvider) : IAloFileReaderFactory
{
    public IAloFileReader<IAloDataContent> GetReader(AloContentInfo contentInfo, Stream dataStream, AloLoadOptions loadOptions)
    {
        switch (contentInfo.Type)
        {
            case AloType.Model:
                return new ModelFileReader(loadOptions, dataStream);
            case AloType.Animation when contentInfo.Version == AloVersion.V1:
                return new AnimationReaderV1(loadOptions, dataStream);
            case AloType.Animation:
                return new AnimationReaderV2(loadOptions, dataStream);
            case AloType.Particle when contentInfo.Version == AloVersion.V1:
                return new ParticleReaderV1(loadOptions, dataStream);
            case AloType.Particle:
                return new ParticleReaderV2(loadOptions, dataStream);
            default:
                throw new NotSupportedException($"ALO content type {contentInfo.Type} is not supported.");
        }
    }
}