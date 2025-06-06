using AnakinRaW.ApplicationBase.Update.Options;

namespace AET.ModVerifyTool.Settings;

internal sealed class ModVerifySettingsContainer
{
    public ModVerifyAppSettings? ModVerifyAppSettings { get; init; }

    public ApplicationUpdateOptions? UpdateOptions { get; init; }

    public bool HasSettings => ModVerifyAppSettings is not null || UpdateOptions is not null;
}