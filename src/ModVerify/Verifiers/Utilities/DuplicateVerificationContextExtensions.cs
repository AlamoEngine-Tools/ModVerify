using AET.ModVerify.Verifiers.Commons;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Files.MTD.Files;
using PG.StarWarsGame.Files.XML.Data;

namespace AET.ModVerify.Verifiers.Utilities;

/// <summary>Provides factory methods for creating <see cref="IDuplicateVerificationContext"/> instances.</summary>
public static class DuplicateVerificationContextExtensions
{
    extension(IDuplicateVerificationContext)
    {
        /// <summary>Creates a duplicate verification context for the entries of an MTD file.</summary>
        /// <param name="mtdFile">The MTD file whose entries are checked for duplicates.</param>
        /// <returns>A new duplicate verification context.</returns>
        public static IDuplicateVerificationContext CreateForMtd(IMtdFile mtdFile)
        {
            return new MtdDuplicateVerificationContext(mtdFile);
        }

        /// <summary>Creates a duplicate verification context for the named entities of a game manager.</summary>
        /// <typeparam name="T">The type of named XML entity.</typeparam>
        /// <param name="gameManager">The game manager whose entities are checked for duplicates.</param>
        /// <param name="databaseName">The name of the database, used for error reporting.</param>
        /// <returns>A new duplicate verification context.</returns>
        public static IDuplicateVerificationContext CreateForNamedXmlObjects<T>(IGameManager<T> gameManager, string databaseName) 
            where T : NamedXmlObject
        {
            return new NamedXmlObjectDuplicateVerificationContext<T>(databaseName, gameManager);
        }
    }
}