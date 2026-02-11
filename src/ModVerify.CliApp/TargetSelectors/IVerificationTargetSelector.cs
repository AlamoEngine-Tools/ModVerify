using AET.ModVerify.App.Settings;

namespace AET.ModVerify.App.TargetSelectors;

internal interface IVerificationTargetSelector
{
    VerificationTarget Select(VerificationTargetSettings settings);
}