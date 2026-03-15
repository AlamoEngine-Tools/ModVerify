using AET.ModVerify.Verifiers.Commons;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Files.MTD.Files;
using PG.StarWarsGame.Files.XML.Data;

namespace AET.ModVerify.Verifiers.Utilities;

public static class DuplicateVerificationContextExtensions
{
    extension(IDuplicateVerificationContext)
    {
        public static IDuplicateVerificationContext CreateForMtd(IMtdFile mtdFile)
        {
            return new MtdDuplicateVerificationContext(mtdFile);
        }

        public static IDuplicateVerificationContext CreateForNamedXmlObjects<T>(IGameManager<T> gameManager, string databaseName) where T : NamedXmlObject
        {
            return new NamedXmlObjectDuplicateVerificationContext<T>(databaseName, gameManager);
        }
    }
}