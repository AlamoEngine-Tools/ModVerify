using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Engine.Xml;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PG.StarWarsGame.Engine.GameObjects;

internal class GameObjectTypeGameManager(GameRepository repository, GameEngineErrorReporterWrapper errorReporter, IServiceProvider serviceProvider)
    : GameManagerBase<GameObject>(repository, errorReporter, serviceProvider), IGameObjectTypeGameManager
{
    protected override async Task InitializeCoreAsync(CancellationToken token)
    {
        Logger?.LogInformation("Parsing GameObjects...");
        await Task.Run(ParseGameObjectDatabases, token);
    }


    private void ParseGameObjectDatabases()
    {
        var parser = new PetroglyphStarWarsGameXmlParser(GameRepository, 
            new PetroglyphStarWarsGameXmlParseSettings
            {
                GameManager = ToString(),
                InvalidFilesListXmlFailsInitialization = true,
                InvalidContainerXmlFailsInitialization = false,
            }, ServiceProvider, ErrorReporter);

        var xmlFileList = parser.ParseFileList(@"DATA\XML\GAMEOBJECTFILES.XML").Files
            .Select(x =>
            {
                var filePath = FileSystem.Path.Combine(@".\DATA\XML\", x);
                VerifyFilePathLength(filePath);
                return filePath;
            }).ToList();


        //var gameObjectFileParser = new GameObjectFileParser(serviceProvider, errorReporter);

        var allLoaded = false;
        for (var passNumber = 0; !allLoaded && passNumber < 10; passNumber++)
        {
            foreach (var gameObjectXmlFile in xmlFileList)
            {
                if (passNumber == 0)
                {
                    //ParseSingleGameObjectFile(gameObjectXmlFile, parser, gameObjectFileParser);
                }
                else
                {
                }
            }



            //GameObjectTypeClass::Static_Post_Load_Fixup();
            //SFXEventReferenceClass::Static_Post_Load_Fixup();
            //SpeechEventReferenceClass::Static_Post_Load_Fixup();
            //MusicEventReferenceClass::Static_Post_Load_Fixup();
            //FactionReferenceClass::Static_Post_Load_Fixup();
            //...
        }
    }

    //private void ParseSingleGameObjectFile(string file, EngineXmlParser engineParser, GameObjectFileParser gameObjectFileParser)
    //{
    //    engineParser.ParseEntriesFromContainerFile(gameObjectFileParser, file, NamedEntries);
    //}

    private void VerifyFilePathLength(string filePath)
    {
        if (filePath.Length > PGConstants.MaxGameObjectDatabaseFileName)
        {
            // Technically this is an assert in the engine, but in Release Mode, the game CTDs.
            // Thus, we rank this as an initialization error.
            ErrorReporter.Report(new InitializationError
            {
                GameManager = ToString(),
                Message = $"Game object file '{filePath}' is longer than {PGConstants.MaxGameObjectDatabaseFileName} characters."
            });
        }
    }
}