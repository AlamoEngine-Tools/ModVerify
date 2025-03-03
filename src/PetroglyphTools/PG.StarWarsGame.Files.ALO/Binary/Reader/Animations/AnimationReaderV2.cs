using System.IO;
using PG.StarWarsGame.Files.ALO.Services;

namespace PG.StarWarsGame.Files.ALO.Binary.Reader.Animations;

internal class AnimationReaderV2(AloLoadOptions loadOptions, Stream stream) : AnimationReaderBase(loadOptions, stream);