using AET.ModVerify.App.Settings;

namespace AET.ModVerify.App.ModSelectors;

internal interface IVerificationTargetSelector
{
    VerificationTarget Select(GameInstallationsSettings settings);
}