using PG.StarWarsGame.Files.ALO.Services;
using System.IO;

namespace PG.StarWarsGame.Files.ALO.Binary.Reader.Animations;

internal class AnimationReaderV1(AloLoadOptions loadOptions, Stream stream) : AnimationReaderBase(loadOptions, stream);