using System;
using System.IO;
using System.Linq;
using System.Xml;
using AnakinRaW.CommonUtilities.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Files.XML;
using PG.StarWarsGame.Files.XML.Data;
using PG.StarWarsGame.Files.XML.ErrorHandling;
using PG.StarWarsGame.Files.XML.Parsers;

namespace PG.StarWarsGame.Engine.Xml;

public sealed class PetroglyphStarWarsGameXmlParser : IPetroglyphXmlParserInfo
{
    private readonly IGameRepository _gameRepository;
    private readonly PetroglyphFileSystem _pgFileSystem;
    private readonly PetroglyphStarWarsGameXmlParseSettings _settings;
    private readonly IGameEngineErrorReporter _reporter;
    private readonly IPetroglyphXmlFileParserFactory _fileParserFactory;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public string Name { get; }

    public PetroglyphStarWarsGameXmlParser(
        IGameRepository gameRepository,
        PetroglyphStarWarsGameXmlParseSettings settings,
        IServiceProvider serviceProvider, 
        IGameEngineErrorReporter reporter)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
        _pgFileSystem = gameRepository.PGFileSystem;
        _fileParserFactory = serviceProvider.GetRequiredService<IPetroglyphXmlFileParserFactory>();
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType()) ?? NullLogger.Instance;
        
        Name = GetType().FullName!;
    }

    public T? ParseFile<T>(string xmlFile, XmlFileParser<T> parser) where T : XmlObject
    {
        return ParseCore<T?>(xmlFile, parser.ParseFile, () => null);
    }

    public XmlFileList ParseFileList(string xmlFile)
    {
        return ParseCore(xmlFile, 
            stream => new XmlFileListParser(_serviceProvider, _reporter).ParseFile(stream),
            () => XmlFileList.Empty(new XmlLocationInfo(xmlFile, null)));
    }

    public void ParseEntriesFromFileListXml<T>(
        string xmlFile,
        string lookupPath,
        FrugalValueListDictionary<Crc32, T> entries,
        Action<string>? onParseContainerAction = null) where T : NamedXmlObject
    {
        var container = ParseFileList(xmlFile);

        var xmlFiles = container.Files.Select(x => _pgFileSystem.CombinePath(lookupPath, x)).ToList();

        var parser = new XmlContainerFileParser<T>(_serviceProvider,
            _fileParserFactory.CreateNamedXmlObjectParser<T>(_gameRepository.EngineType, _reporter), _reporter);

        foreach (var file in xmlFiles)
        {
            onParseContainerAction?.Invoke(file);
            if (!ParseObjectsFromContainerFile(file, parser, entries))
                return;
        }
    }
    
    public bool ParseObjectsFromContainerFile<T>(
        string xmlFile,
        IXmlContainerFileParser<T> parser,
        IFrugalValueListDictionary<Crc32, T> entries) where T : NamedXmlObject
    {
        return ParseCore(xmlFile, stream =>
        {
            parser.ParseFile(stream, entries);
            return true;
        }, () => _settings.InvalidObjectXmlFailsInitialization);
    }

    private T ParseCore<T>(string xmlFile, Func<Stream, T> parseAction, Func<T> invalidFileAction)
    {
        _logger.LogDebug("Parsing file '{XmlFile}'", xmlFile);

        using var fileStream = _gameRepository.TryOpenFile(xmlFile);

        if (fileStream is null)
        {
            var message = $"Could not find XML file '{xmlFile}'";
            _logger.LogWarning(message);

            _reporter.Report(new XmlError(this, locationInfo: new XmlLocationInfo(xmlFile, null))
            {
                Message = message,
                ErrorKind = XmlParseErrorKind.MissingFile
            });

            if (_settings.InvalidObjectXmlFailsInitialization)
            {
                _reporter.Report(new InitializationError
                {
                    GameManager = _settings.GameManager,
                    Message = message,
                });
            }

            return invalidFileAction();
        }

        try
        {
            return parseAction(fileStream);
        }
        catch (XmlException e)
        {
            _reporter.Report(new XmlError(this, locationInfo: new XmlLocationInfo(xmlFile, e.LineNumber))
            {
                ErrorKind = XmlParseErrorKind.Unknown,
                Message = e.Message,
            });
            if (_settings.InvalidObjectXmlFailsInitialization)
            {
                _reporter.Report(new InitializationError
                {
                    GameManager = _settings.GameManager,
                    Message = e.Message,
                });
            }

            return invalidFileAction();
        }
    }
}

enum XmlDataType
{
    DB_DATA_TYPE_BOOL = 0x0,
    DB_DATA_TYPE_DWORD = 0x1,
    DB_DATA_TYPE_UNSIGNED_CHAR = 0x2,
    DB_DATA_TYPE_SIGNED_CHAR = 0x3,
    DB_DATA_TYPE_UNSIGNED_CHAR_PERCENT = 0x4,
    DB_DATA_TYPE_UNSIGNED_INT = 0x5,
    DB_DATA_TYPE_SIGNED_INT = 0x6,
    DB_DATA_TYPE_UNSIGNED_INT_PERCENT = 0x7,
    DB_DATA_TYPE_FLOAT = 0x8,
    DB_DATA_TYPE_UNIT_FLOAT = 0x9,
    DB_DATA_TYPE_DOUBLE = 0xa,
    DB_DATA_TYPE_UNIT_DOUBLE = 0xb,
    DB_DATA_TYPE_SIGNED_INT_HEX = 0xc,
    DB_DATA_TYPE_DWORD_HEX = 0xd,
    DB_DATA_TYPE_CONVERSION = 0xe,
    DB_DATA_TYPE_VECTOR2 = 0xf,
    DB_DATA_TYPE_VECTOR3 = 0x10,
    DB_DATA_TYPE_VECTOR4 = 0x11,
    DB_DATA_TYPE_DYN_VECTOR_INT = 0x12,
    DB_DATA_TYPE_DYN_VECTOR_FLOAT = 0x13,
    DB_DATA_TYPE_DYN_VECTOR_VECTOR3 = 0x14,
    DB_DATA_TYPE_DYN_VECTOR_VECTOR2 = 0x15,
    DB_DATA_TYPE_RGBA = 0x16,
    DB_DATA_TYPE_STL_STRING = 0x17,
    DB_DATA_TYPE_STL_LIST_STL_STRINGS = 0x18,
    DB_DATA_TYPE_STL_VECTOR_DWORDS = 0x19,
    DB_DATA_TYPE_STL_VECTOR_DWORDS_HEX = 0x1a,
    DB_DATA_TYPE_STL_VECTOR_STL_STRINGS = 0x1b,
    DB_DATA_TYPE_STL_STRING_UPPER = 0x1c,
    DB_DATA_TYPE_OBJECT_REFERENCE = 0x1d,
    DB_DATA_TYPE_MULTI_OBJECT_REFERENCE = 0x1e,
    DB_DATA_TYPE_SFX_EVENT = 0x1f,
    DB_DATA_TYPE_SPEECH_EVENT = 0x20,
    DB_DATA_TYPE_MUSIC_EVENT = 0x21,
    DB_DATA_TYPE_SFXEVENT_OVERRIDE_LIST_ENTRY = 0x22,
    DB_DATA_TYPE_WEATHER_SFXEVENT_LOOP_LIST_ENTRY = 0x23,
    DB_DATA_TYPE_WEATHER_SFXEVENT_INTERMITTENT_LIST_ENTRY = 0x24,
    DB_DATA_TYPE_WEATHER_SFX_EVENT_PAIR_ARRAY_ENTRY = 0x25,
    DB_DATA_TYPE_AMBIENT_SFXEVENT_INTERMITTENT_LIST_ENTRY = 0x26,
    DB_DATA_TYPE_DYN_VECTOR_SFX_EVENT_ENTRY = 0x27,
    DB_DATA_TYPE_TRIPLE_OBJ_TYPE_AND_SPEECH_EVENT_ENTRY = 0x28,
    DB_DATA_TYPE_DYN_VECTOR_FACTION_AND_MUSIC_EVENT_PAIR_ENTRY = 0x29,
    DB_DATA_TYPE_DYN_VECTOR_STL_STRINGS = 0x2a,
    DB_DATA_TYPE_FACTION_DATA_OVERRIDE_UINT = 0x2b,
    DB_DATA_TYPE_FACTION_DATA_OVERRIDE_FLOAT = 0x2c,
    DB_DATA_TYPE_FACTION_DATA_OVERRIDE_NAMEREF = 0x2d,
    DB_DATA_TYPE_QUADRATIC = 0x2e,
    DB_DATA_TYPE_SPLINE = 0x2f,
    DB_DATA_TYPE_LINEAR = 0x30,
    DB_DATA_TYPE_PARABOLIC = 0x31,
    DB_DATA_TYPE_SHIPCLASS = 0x32,
    DB_DATA_TYPE_SCRIPT_VARIABLE = 0x33,
    DB_DATA_TYPE_CONVERSION_STRING_PAIR_VECTOR = 0x34,
    DB_DATA_TYPE_CONVERSION_OBJECT_REF_VECTOR = 0x35,
    DB_DATA_TYPE_STL_VECTOR_OBJECT_REFERENCE_STRING_PAIR = 0x36,
    DB_DATA_TYPE_STARTING_FORCE_DEFINITION = 0x37,
    DB_DATA_TYPE_STL_VECTOR_ABILITIES = 0x38,
    DB_DATA_TYPE_STL_VECTOR_ABILITIES_DATA = 0x39,
    DB_DATA_TYPE_STL_VECTOR_ACTIONS = 0x3a,
    DB_DATA_TYPE_STL_HASHMAP_DAMAGE_TO_DEATH_CLONE = 0x3b,
    DB_DATA_TYPE_STL_VECTOR_STRING_INT_PAIR = 0x3c,
    DB_DATA_TYPE_LIST_GAME_OBJECT_CATEGORY_FLOAT_PAIR = 0x3d,
    DB_DATA_TYPE_DAMAGE_TO_ARMOR_MOD_ENTRY = 0x3e,
    DB_DATA_QUOTED_STRING_DYN_VECTOR_ENTRY = 0x3f,
    DB_DATA_TYPE_LANGUAGE_STL_STRING_ARRAY_ENTRY = 0x40,
    DB_DATA_TYPE_HARD_POINT_TYPE_ARRAY_OF_DYN_VECTOR_STL_STRINGS = 0x41,
    DB_DATA_TYPE_HARD_POINT_TYPE_ARRAY_OF_SFXEVENTS = 0x42,
    DB_DATA_TYPE_FACTION = 0x43,
    DB_DATA_TYPE_WEIGHTED_TYPE_LIST = 0x44,
    DB_DATA_TYPE_STL_VECTOR_CONVERSION = 0x45,
    DB_DATA_TYPE_STL_VECTOR_OBJECT_REFERENCE_INT_PAIR = 0x46,
    DB_DATA_TYPE_DISCRETE_DISTRIBUTION_NAME_REFERENCE = 0x47,
    DB_DATA_TYPE_STL_HASHMAP_CONVERSION_TO_FLOAT = 0x48,
    DB_DATA_TYPE_UNIT_ABILITY = 0x49,
    DB_TYPE_PROJECTILE_CATEGORY = 0x4a,
    DB_DATA_TYPE_RENDER_MODE = 0x4b,
    DB_DATA_TYPE_NON_HERO_ABILITIES_SFX_EVENT_PAIR = 0x4c,
    DB_DATA_TYPE_UNIT_MODE_MULTIPLIER_MOD = 0x4d,
    DB_DATA_TYPE_UNIT_MODE_FLAG_MOD = 0x4e,
    DB_DATA_TYPE_STRING_INT_PAIR = 0x4f,
    DB_DATA_TYPE_BIT_BOOL = 0x50,
    DB_DATA_TYPE_STL_VECTOR_PROJECTILE_CATEGORIES = 0x51,
    DB_DATA_TYPE_COMBAT_MOD = 0x52,
    DB_DATA_TYPE_MULTI_OBJECT_REFERENCE_WITH_OR = 0x53,
    DB_DATA_TYPE_CONVERSION_PTR = 0x54,
    DB_DATA_TYPE_CONVERSION_OBJECT_REF_VECTOR_PTR = 0x55,
    DB_DATA_TYPE_CONVERSION_64 = 0x56
}