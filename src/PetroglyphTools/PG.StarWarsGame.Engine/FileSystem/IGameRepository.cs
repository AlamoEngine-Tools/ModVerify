namespace PG.StarWarsGame.Engine.FileSystem;

public interface IGameRepository : IRepository
{
   IRepository EffectsRepository { get; }

   bool FileExists(string filePath, string[] extensions, bool megFileOnly = false);
}