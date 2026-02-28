using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Engine.Xml;
using PG.StarWarsGame.Engine.Xml.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PG.StarWarsGame.Engine.GameObjects;

internal class GameObjectTypeGameManager : GameManagerBase<GameObject>, IGameObjectTypeGameManager
{
    private readonly List<GameObject> _gameObjects;

    public GameObjectTypeGameManager(
        GameEngineType engineType,
        GameRepository repository, 
        GameEngineErrorReporterWrapper errorReporter, 
        IServiceProvider serviceProvider)
        : base(engineType, repository, errorReporter, serviceProvider)
    {
        _gameObjects = new List<GameObject>();
        GameObjects = new ReadOnlyCollection<GameObject>(_gameObjects);
    }


    public IReadOnlyList<GameObject> GameObjects
    {
        get
        {
            ThrowIfNotInitialized();
            return field;
        }
    }

    protected override async Task InitializeCoreAsync(CancellationToken token)
    {
        Logger?.LogInformation("Parsing GameObjects...");
        await Task.Run(ParseGameObjectDatabases, token);
    }

    private void ParseGameObjectDatabases()
    {
        var gameParser = new PetroglyphStarWarsGameXmlParser(GameRepository, 
            new PetroglyphStarWarsGameXmlParseSettings
            {
                GameManager = ToString(),
                InvalidFilesListXmlFailsInitialization = true,
                InvalidObjectXmlFailsInitialization = false,
            }, ServiceProvider, ErrorReporter);

        var xmlFileList = gameParser.ParseFileList(@"DATA\XML\GAMEOBJECTFILES.XML").Files
            .Select(x => FileSystem.Path.Combine(@".\DATA\XML\", x))
            .Where(VerifyFilePathLength)
            .ToList();

        var gameObjectFileParser = new GameObjectFileParser(ServiceProvider, ErrorReporter);

        var allLoaded = false;

        // This also acts a guard against infinite loops in case of unexpected circular dependencies or
        // when a unit declares itself as its own <Variant_Of_Existing_Type>
        for (var passNumber = 0; !allLoaded && passNumber < 10; passNumber++)
        {
            Logger?.LogDebug("***** Parsing game object types - pass {PassNumber} *****", passNumber);

            if (passNumber != 0) 
                gameObjectFileParser.OverlayLoad = true;
            
            foreach (var gameObjectXmlFile in xmlFileList)
            {
                if (passNumber == 0)
                {
                    try
                    {
                        gameObjectFileParser.GameObjectParsed += OnGameObjectParsed;
                        ParseSingleGameObjectFile(gameObjectXmlFile, gameParser, gameObjectFileParser);
                    }
                    finally
                    {
                        gameObjectFileParser.GameObjectParsed -= OnGameObjectParsed;
                    }
                }
                else
                {
                    foreach (var gameObject in _gameObjects)
                    {
                        if (!gameObject.IsLoadingComplete && gameObject.Location.XmlFile == gameObjectXmlFile)
                            ParseSingleGameObjectFile(gameObjectXmlFile, gameParser, gameObjectFileParser);
                    }
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

    private void OnGameObjectParsed(object sender, GameObjectParsedEventArgs e)
    {
        if (!e.Unique)
        {
            var entries = NamedEntries.GetValues(e.ParsedElement.Crc32)
                .Select(x => x.Name);
            ErrorReporter.Assert(EngineAssert.Create(
                EngineAssertKind.DuplicateEntry, 
                e.ParsedElement.Crc32, entries, 
                $"Error: Game object type {e.ParsedElement.Name} is defined multiple times."));
        }

        if (NamedEntries.ValueCount >= 0x10000)
        {
            ErrorReporter.Assert(
                EngineAssert.Create(
                    EngineAssertKind.ValueOutOfRange, 
                    NamedEntries.ValueCount,
                    [ToString()], 
                    "Too many game object types defined."));
        }
    }

    private void ParseSingleGameObjectFile(
        string file,
        PetroglyphStarWarsGameXmlParser gameParser,
        GameObjectFileParser gameObjectFileParser)
    {
        gameParser.ParseObjectsFromContainerFile(file, gameObjectFileParser, NamedEntries);
    }

    private bool VerifyFilePathLength(string filePath)
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
            return false;
        }

        return true;
    }
}