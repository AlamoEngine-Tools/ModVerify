using PG.StarWarsGame.Files.ChunkFiles.Files;
using PG.StarWarsGame.Files.TED.Data;

namespace PG.StarWarsGame.Files.TED.Files;

public interface ITedFile : IChunkFile<IMapData, TedFileInformation>;