using AnakinRaW.CommonUtilities.Collections;
using Microsoft.Extensions.Logging;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine.CommandBar.Components;
using PG.StarWarsGame.Engine.CommandBar.Xml;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.Rendering;
using PG.StarWarsGame.Engine.Xml;
using PG.StarWarsGame.Files.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace PG.StarWarsGame.Engine.CommandBar;

internal partial class CommandBarGameManager
{ 
    protected override async Task InitializeCoreAsync(CancellationToken token)
    {
        Logger?.LogInformation("Creating command bar components...");

        var contentParser = new PetroglyphStarWarsGameXmlParser(GameRepository, new PetroglyphStarWarsGameXmlParseSettings
        {
            GameManager = ToString(),
            InvalidObjectXmlFailsInitialization = true,
            InvalidFilesListXmlFailsInitialization = true
        }, ServiceProvider, ErrorReporter);

        var parsedCommandBarComponents = new FrugalValueListDictionary<Crc32, CommandBarComponentData>();

        await Task.Run(() => contentParser.ParseEntriesFromFileListXml(
                ".\\Data\\XML\\CommandBarComponentFiles.xml",
                ".\\DATA\\XML\\",
                parsedCommandBarComponents,
                VerifyFilePathLength),
            token);

        CommandBarOffset = new Vector3(-0.5f, -0.5f, 0.0f);

        // Create Scene
        // Create Camera
        // Resize(force: true)

        foreach (var parsedCommandBarComponent in parsedCommandBarComponents.Values)
        {
            var component = CommandBarBaseComponent.Create(parsedCommandBarComponent, ErrorReporter);
            if (component is not null)
            {
                var crc = _hashingService.GetCrc32(component.Name, PGConstants.DefaultPGEncoding);
                NamedEntries.Add(crc, component);
            }

            if (component is CommandBarShellComponent shellComponent) 
                SetModelTransform(shellComponent);
        }

        SetComponentGroup(Components);
        SetMegaTexture();
        SetDefaultFont();

        LinkComponentsToShell();
        LinkComponentsWithActions();


        // CommandBarClass::Set_Encyclopedia_Delay_Time(this);
        // CommandBarClass::Find_Neighbors(this);

        // CommandBarClass::Load_Hero_Particles(this);
        // CommandBarClass::Load_Corruption_Particle(this);
    }

    private void SetModelTransform(CommandBarShellComponent shellComponent)
    {
        if (string.IsNullOrEmpty(shellComponent.ModelPath) || 
            !GameRepository.ModelRepository.FileExists(shellComponent.ModelPath))
            return;
        shellComponent.SetOffsetAndScale(CommandBarOffset, CommandBarScale);
    }

    private void LinkComponentsWithActions()
    {
        var nameLookup = SupportedCommandBarComponentData.GetComponentIdsForEngine(GameRepository.EngineType);
        foreach (var idPair in nameLookup)
        {
            // The engine does not uppercase the name here
            var crc = _hashingService.GetCrc32(idPair.Value, PGConstants.DefaultPGEncoding);
            if (NamedEntries.TryGetFirstValue(crc, out var component))
            {
                // NB: Currently we do not have "action"
                // but we keep the original method name 'LinkComponentsWithActions'
                component.Id = idPair.Key;
            }
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
                EngineAssert.FromNullOrEmpty(
                    [component.Name], $"Cannot link component '{component}' because shell component is null."));
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
            model = pgRender.LoadModelAndAnimations(modelPath.AsSpan(), null, true);
            modelCache.Add(shell.Name, model);
        }

        if (model is null)
        {
            ErrorReporter.Assert(
                EngineAssert.FromNullOrEmpty(
                    [$"component='{component.Name}'", $"shell='{shell.Name}'"], 
                    $"Cannot link component '{componentName}' to shell '{shell.Name}' because model '{modelPath}' could not be loaded."));
            return false;
        }

        if (!model.IsModel)
        {
            ErrorReporter.Assert(
                EngineAssert.FromNullOrEmpty(
                    [$"component='{component.Name}'", $"shell='{shell.Name}'"],
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
            var fontName = PGConstants.DefaultUnicodeFontName;
            var size = 11;
            var font = fontManager.CreateFont(fontName, size, true, false, false, 1.0f);
            if (font is null)
                ErrorReporter.Assert(EngineAssert.FromNullOrEmpty([ToString()], $"Unable to create Default from name {fontName}"));
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
            MtdFile = megaTexture is null ? null : _mtdFileService.Load(megaTexture);
        }
        catch (BinaryCorruptedException e)
        {
            var message = $"Failed to load MTD file '{mtdPath}': {e.Message}";
            Logger?.LogError(e, message);
            ErrorReporter.Assert(EngineAssert.Create(EngineAssertKind.CorruptBinary, mtdPath, [], message));
        }
        
        GameRepository.TextureRepository.FileExists($"{CommandBarConstants.MegaTextureBaseName}.tga", false, out _, out var actualFilePath);
        MegaTextureFileName = FileSystem.Path.GetFileName(actualFilePath);
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