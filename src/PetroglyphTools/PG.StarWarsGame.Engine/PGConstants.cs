using System.Text;

namespace PG.StarWarsGame.Engine;

public static class PGConstants
{
    public static readonly Encoding DefaultPGEncoding = Encoding.ASCII;

    // Always reserve one character for the null-terminator
    public const int MaxMegEntryPathLength = 259;
    public const int MaxEffectFileName = 259;
    public const int MaxTextureFileName = 259;
    public const int MaxModelFileName = 259;
    public const int MaxSFXEventDatabaseFileName = 259;
    public const int MaxSFXEventName = 255;
    public const int MaxGameObjectDatabaseFileName = 127;
    public const int MaxCommandBarDatabaseFileName = 259;

    public const int MaxGuiDialogMegaTextureFileName = 255;
}