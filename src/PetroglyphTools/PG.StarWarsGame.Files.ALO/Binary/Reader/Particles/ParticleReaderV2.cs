﻿using System;
using System.IO;
using PG.StarWarsGame.Files.ALO.Data;
using PG.StarWarsGame.Files.ALO.Services;

namespace PG.StarWarsGame.Files.ALO.Binary.Reader.Particles;

internal class ParticleReaderV2(AloLoadOptions loadOptions, Stream stream) : AloFileReader<AlamoParticle>(loadOptions, stream)
{
    public override AlamoParticle Read()
    {
        throw new NotImplementedException();
    }
}