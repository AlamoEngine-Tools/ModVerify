using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.Commons.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.CommandBar.Components;
using PG.StarWarsGame.Engine.CommandBar.Xml;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Engine.Xml.Parsers;
using PG.StarWarsGame.Files.MTD.Files;
using PG.StarWarsGame.Files.MTD.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PG.StarWarsGame.Engine.CommandBar;

internal class CommandBarGameManager(
    GameRepository repository,
    GameErrorReporterWrapper errorReporter,
    IServiceProvider serviceProvider)
    : GameManagerBase<CommandBarBaseComponent>(repository, errorReporter, serviceProvider), ICommandBarGameManager
{
    public const string MegaTextureBaseName = "MT_COMMANDBAR";

    private readonly ICrc32HashingService _hashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();
    private readonly IMtdFileService _mtdFileService = serviceProvider.GetRequiredService<IMtdFileService>();
    private readonly Dictionary<string, CommandBarComponentGroup> _groups = new();

    private bool _megaTextureExists;

    public ICollection<CommandBarBaseComponent> Components => Entries;

    public IReadOnlyDictionary<string, CommandBarComponentGroup> Groups => _groups;

    public IMtdFile? MegaTextureFile
    {
        get
        {
            ThrowIfNotInitialized();
            return field;
        }
        private set
        {
            ThrowIfAlreadyInitialized();
            field = value;
        }
    }

    protected override async Task InitializeCoreAsync(CancellationToken token)
    {
        Logger?.LogInformation("Creating command bar components...");

        var contentParser = new XmlContainerContentParser(ServiceProvider, ErrorReporter);
        contentParser.XmlParseError += OnParseError;

        var parsedCommandBarComponents = new ValueListDictionary<Crc32, CommandBarComponentData>();

        try
        {
            await Task.Run(() => contentParser.ParseEntriesFromFileListXml(
                    "DATA\\XML\\CommandBarComponentFiles.XML",
                    GameRepository,
                    ".\\DATA\\XML",
                    parsedCommandBarComponents,
                    VerifyFilePathLength),
                token);
        }
        finally
        {
            contentParser.XmlParseError -= OnParseError;
        }

        // Create Scene
        // Create Camera
        // Resize(true)

        foreach (var parsedCommandBarComponent in parsedCommandBarComponents.Values)
        {
            var component = CommandBarBaseComponent.Create(parsedCommandBarComponent, ErrorReporter);
            if (component is not null)
            {
                var crc = _hashingService.GetCrc32(component.Name, PGConstants.DefaultPGEncoding);
                NamedEntries.Add(crc, component);
            }
        }

        SetComponentGroup(Components);
        SetMegaTexture();
        SetDefaultFont();

        LinkComponentsToShell();
        LinkComponentsWithActions();
    }

    private void LinkComponentsWithActions()
    {
        var ids = (CommandBarComponentId[])Enum.GetValues(typeof(CommandBarComponentId));
        foreach (var id in ids)
        {
            if (!SupportedCommandBarComponentData.SupportedComponents.TryGetValue(id, out var name))
                continue;
            var crc = _hashingService.GetCrc32(name, PGConstants.DefaultPGEncoding);
            if (NamedEntries.TryGetFirstValue(crc, out var component))
                component.Id = id;
        }
    }

    private void LinkComponentsToShell()
    {
        // TODO: DEBUG!!!

        if (!Groups.TryGetValue("Shells", out var shellGroup))
            return;

        foreach (var shellComponent in shellGroup.Components)
        {
            if (shellComponent.Type == CommandBarComponentType.None)
                continue;

            foreach (var other in shellGroup.Components)
            {
                
            }
        }
    }

    private void SetDefaultFont()
    {
        // The code is only triggered iff at least one Text CommandbarBar component existed
        if (Components.FirstOrDefault(x => x is CommandBarTextComponent or CommandBarTextButtonComponent) is null)
            return;

        /*
           140ab82bd        if (!BaseComponentClass::DefaultFont)
           140ab82bd        {
           140ab82c6            int32_t point_size = j_GameConstantsClass::Get_Command_Bar_Default_Font_Size(&TheGameConstants);
           140ab830c            BaseComponentClass::DefaultFont = j_FontManagerClass::Create_Font(&FontManager, j_GameConstantsClass::Get_Command_Bar_Default_Font_Name(&TheGameConstants), point_size, 1, 0, 0, 1f);
           140ab830c            
           140ab831b            if (!BaseComponentClass::DefaultFont)
           140ab8331                j_Assert_Handler("DefaultFont != NULL", "C:\BuildSystem\AB_SW_STEAM_FOC_BUILDSERV04_BUILD\StarWars_Steam\FOC\Code\RTS\CommandBar\CommandBarComponent.cpp", 0x1325);
           140ab82bd        } 
         */
    }

    private void SetMegaTexture()
    {
        // The code is only triggered iff at least one Shell CommandbarBar component existed
        if (Components.FirstOrDefault(x => x is CommandBarShellComponent) is null)
            return;
        // Note: The tag <Mega_Texture_Name> is not used by the engine
        var mtdPath = FileSystem.Path.Combine("DATA\\ART\\TEXTURES", $"{MegaTextureBaseName}.mtd");
        using var megaTexture = GameRepository.TryOpenFile(mtdPath);
        MegaTextureFile = megaTexture is null ? null : _mtdFileService.Load(megaTexture);
        _megaTextureExists = GameRepository.TextureRepository.FileExists($"{MegaTextureBaseName}.tga");
    }

    private void SetComponentGroup(IEnumerable<CommandBarBaseComponent> components)
    {
        var groupData = components
            .Where(x => !string.IsNullOrEmpty(x.XmlData.Group))
            .GroupBy(x => x.XmlData.Group!, StringComparer.Ordinal);

        foreach (var grouping in groupData)
        {
            var group = new CommandBarComponentGroup(grouping.Key, grouping);
            _groups.Add(grouping.Key, group);
            foreach (var component in grouping) 
                component.Group = group;
        }
    }

    private void OnParseError(object sender, XmlContainerParserErrorEventArgs e)
    {
        if (e.ErrorInXmlFileList || e.HasException)
        {
            e.Continue = false;
            ErrorReporter.Report(new InitializationError
            {
                GameManager = ToString(),
                Message = GetMessage(e)
            });
        }
    }

    private static string GetMessage(XmlContainerParserErrorEventArgs errorEventArgs)
    {
        if (errorEventArgs.HasException)
            return $"Error while parsing CommandBar XML file '{errorEventArgs.File}': {errorEventArgs.Exception.Message}";
        return "Could not find CommandBarComponentFiles.xml";
    }

    private void VerifyFilePathLength(string filePath)
    {
        if (filePath.Length > PGConstants.MaxCommandBarDatabaseFileName)
        {
            ErrorReporter.Report(new InitializationError
            {
                GameManager = ToString(),
                Message = $"CommandBar file '{filePath}' is longer than {PGConstants.MaxCommandBarDatabaseFileName} characters."
            });
        }
    }
}

