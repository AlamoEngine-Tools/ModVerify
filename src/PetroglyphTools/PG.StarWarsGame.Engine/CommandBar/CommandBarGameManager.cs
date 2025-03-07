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
using PG.StarWarsGame.Engine.GameConstants;
using PG.StarWarsGame.Engine.Rendering;
using PG.StarWarsGame.Engine.Rendering.Font;
using PG.StarWarsGame.Files.Binary;

namespace PG.StarWarsGame.Engine.CommandBar;

internal class CommandBarGameManager(
    GameRepository repository,
    PGRender pgRender,
    IGameConstants gameConstants,
    IFontManager fontManager,
    GameErrorReporterWrapper errorReporter,
    IServiceProvider serviceProvider)
    : GameManagerBase<CommandBarBaseComponent>(repository, errorReporter, serviceProvider), ICommandBarGameManager
{
    private readonly ICrc32HashingService _hashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();
    private readonly IMtdFileService _mtdFileService = serviceProvider.GetRequiredService<IMtdFileService>();
    private readonly Dictionary<string, CommandBarComponentGroup> _groups = new();
    private readonly PGRender _pgRender = pgRender;

    private bool _megaTextureExists;
    private FontData? _defaultFont;

    public ICollection<CommandBarBaseComponent> Components => Entries;

    public IReadOnlyDictionary<string, CommandBarComponentGroup> Groups => _groups;

    public FontData? DefaultFont 
    {
        get
        {
            ThrowIfNotInitialized();
            return _defaultFont;
        }
        private set
        {
            ThrowIfAlreadyInitialized();
            _defaultFont = value;
        }
    }

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
        if (!Groups.TryGetValue(CommandBarConstants.ShellGroupName, out var shellGroup))
            return;

        var modelCache = new Dictionary<string, ModelClass?>();
        foreach (var component in Components)
        {
            if (component.Type == CommandBarComponentType.Shell)
                continue;

            foreach (var shellComponent in shellGroup.Components)
            {
                if (LinkToShell(component, shellComponent as CommandBarShellComponent, modelCache))
                    break;
            }
        }

        foreach (var model in modelCache.Values) 
            model?.Dispose();
    }

    private bool LinkToShell(
        CommandBarBaseComponent component, 
        CommandBarShellComponent? shell,
        IDictionary<string, ModelClass?> modelCache)
    {
        if (shell is null)
        {
            ErrorReporter.Assert(
                EngineAssert.FromNullOrEmpty(component, 
                    $"Cannot link component '{component}' because shell component is null."));
            return false;
        }

        var componentName = component.Name;
        if (string.IsNullOrEmpty(componentName))
            return false;

        var modelPath = shell.ModelPath;
        if (string.IsNullOrEmpty(modelPath))
            return false;

        if (!modelCache.TryGetValue(shell.Name, out var model))
        {
            model = _pgRender.LoadModelAndAnimations(modelPath.AsSpan(), null, true);
            modelCache.Add(shell.Name, model);
        }

        if (model is null)
        {
            ErrorReporter.Assert(
                EngineAssert.FromNullOrEmpty(new ComponentLinkTuple(component, shell), 
                    $"Cannot link component '{componentName}' to shell '{shell.Name}' because model '{modelPath}' could not be loaded."));
            return false;
        }

        if (!model.IsModel)
        {
            ErrorReporter.Assert(
                EngineAssert.FromNullOrEmpty(new ComponentLinkTuple(component, shell),
                    $"Cannot link component '{componentName}' to shell '{shell.Name}' because the loaded file '{modelPath}' is not a model."));
            return false;
        }

        var boneIndex = model.IndexOfBone(componentName);

        if (boneIndex == -1)
            return false;
        component.Bone = boneIndex;
        component.ParentShell = shell;
        return true;
    }

    private void SetDefaultFont()
    {
        // The code is only triggered iff at least one Text CommandbarBar component existed
        if (Components.FirstOrDefault(x => x is CommandBarTextComponent or CommandBarTextButtonComponent) is null)
            return;

        if (_defaultFont is null)
        {
            // TODO: From GameConstants
            string fontName = PGConstants.DefaultUnicodeFontName;
            int size = 11;
            var font = fontManager.CreateFont(fontName, size, true, false, false, 1.0f);
            if (font is null)
                ErrorReporter.Assert(EngineAssert.FromNullOrEmpty(this, $"Unable to create Default from name {fontName}"));
            DefaultFont = font;
        }
    }

    private void SetMegaTexture()
    {
        // The code is only triggered iff at least one Shell CommandbarBar component existed
        if (Components.FirstOrDefault(x => x is CommandBarShellComponent) is null)
            return;
        // Note: The tag <Mega_Texture_Name> is not used by the engine
        var mtdPath = FileSystem.Path.Combine("DATA\\ART\\TEXTURES", $"{CommandBarConstants.MegaTextureBaseName}.mtd");
        using var megaTexture = GameRepository.TryOpenFile(mtdPath);

        try
        {
            MegaTextureFile = megaTexture is null ? null : _mtdFileService.Load(megaTexture);
        }
        catch (BinaryCorruptedException e)
        {
            var message = $"Failed to load MTD file '{mtdPath}': {e.Message}";
            Logger?.LogError(e, message);
            ErrorReporter.Assert(EngineAssert.Create(EngineAssertKind.CorruptBinary, mtdPath, null, message));
        }
        _megaTextureExists = GameRepository.TextureRepository.FileExists($"{CommandBarConstants.MegaTextureBaseName}.tga");
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

    private sealed class ComponentLinkTuple(CommandBarBaseComponent component, CommandBarShellComponent shell)
    {
        public CommandBarBaseComponent Component { get; } = component;
        public CommandBarShellComponent Shell { get; } = shell;

        public override string ToString()
        {
            return $"component='{Component.Name}' - shell='{Shell.Name}'";
        }
    }
}
