using System;
using System.Collections.Generic;
using System.Threading;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine.Database;

namespace AET.ModVerify.Verifiers;
public sealed partial class ReferencedTexturesVerifier(
    IGameDatabase gameDatabase,
    GameVerifySettings settings,
    IServiceProvider serviceProvider)
    :
        GameVerifierBase(gameDatabase, settings, serviceProvider)
{
    public const string MtdNotFound = "TEX00";
    public const string TexutreNotFound = "TEX01";
    public const string FileNameTooLong = "PAT00";

    protected override void RunVerification(CancellationToken token)
    {
        var textures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            VerifyGuiTextures(textures);
        }
        finally
        {
            textures.Clear();
        }
       
    }
}