using AnakinRaW.ApplicationBase.Update.Options;

namespace AET.ModVerify.App.Settings.CommandLine;

internal sealed class ModVerifyOptionsContainer
{
    public BaseModVerifyOptions? ModVerifyOptions { get; init; }

    public ApplicationUpdateOptions? UpdateOptions { get; init; }

    public bool HasOptions => ModVerifyOptions is not null || UpdateOptions is not null;
}