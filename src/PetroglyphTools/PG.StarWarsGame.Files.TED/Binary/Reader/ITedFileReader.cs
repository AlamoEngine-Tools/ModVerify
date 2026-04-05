using PG.StarWarsGame.Files.ChunkFiles.Binary.Reader;
using PG.StarWarsGame.Files.TED.Data;

namespace PG.StarWarsGame.Files.TED.Binary.Reader;

internal interface ITedFileReader : IChunkFileReader<IMapData>;