using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine.Database;

namespace AET.ModVerify.Verifiers;

public sealed class ReferencedTexturesVerifier(
    IGameDatabase gameDatabase,
    GameVerifySettings settings,
    IServiceProvider serviceProvider) :
    GameVerifierBase(gameDatabase, settings, serviceProvider)
{
    protected override void RunVerification(CancellationToken token)
    {
        var textures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var textureInfo in GetReferencesTexturesFromDatabase())
        {
            if (!textures.Add(textureInfo.Texture))
                continue;
            VerifyTextureExists(textureInfo);
        }
    }

    private void VerifyTextureExists(TextureFinderInfo texture)
    {

        // String buffer is only 64bytes long? Test with longer names!
        // GUIDialogs *may* have their own mtd file. this is defined by the XML attribute. The file is prefixed to "data/art/textures"
        // Other things like unit icons must always be in data/art/textures/MT_commandbar
    }

    private IEnumerable<TextureFinderInfo> GetReferencesTexturesFromDatabase()
    {
        return Database.GameObjectManager.Entries
            .SelectMany(x => x.Models)
            .Select(x => new TextureFinderInfo
            {
                Texture = x,
                SearchInMegaTexture = false, SearchInRepo = false
            });
    }


    private struct TextureFinderInfo
    {
        public required string Texture { get; init; }
        public bool SearchInMegaTexture { get; init; }
        public bool SearchInRepo { get; init; }
    }
}