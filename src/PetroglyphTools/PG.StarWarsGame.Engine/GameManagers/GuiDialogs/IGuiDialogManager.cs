using System.Collections.Generic;
using PG.StarWarsGame.Engine.GuiDialog;
using PG.StarWarsGame.Engine.GuiDialog.Xml;
using PG.StarWarsGame.Files.MTD.Files;

namespace PG.StarWarsGame.Engine.GameManagers;

public interface IGuiDialogManager
{
    IMtdFile? MtdFile { get; }
    
    GuiDialogsXml? GuiDialogsXml { get; }

    IReadOnlyCollection<string> Components { get; }

    IReadOnlyDictionary<GuiComponentType, ComponentTextureEntry> DefaultTextureEntries { get; }

    IReadOnlyDictionary<GuiComponentType, ComponentTextureEntry> GetTextureEntries(string component, out bool componentExist);

    bool TryGetTextureEntry(string component, GuiComponentType key, out ComponentTextureEntry texture);

    bool TextureExists(
        in ComponentTextureEntry textureInfo,
        out GuiTextureOrigin textureOrigin,
        out bool isNone,
        bool buttonMiddleInRepoMode = false);
}