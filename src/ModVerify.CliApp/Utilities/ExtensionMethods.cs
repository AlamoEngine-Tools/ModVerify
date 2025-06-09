using AET.ModVerify.App.Settings.CommandLine;
using AnakinRaW.ApplicationBase.Environment;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure.Games;

namespace AET.ModVerify.App.Utilities;

internal static class ExtensionMethods
{
    public static GameEngineType ToEngineType(this GameType type)
    {
        return type == GameType.Foc ? GameEngineType.Foc : GameEngineType.Eaw;
    }

    public static GameType FromEngineType(this GameEngineType type)
    {
        return type == GameEngineType.Foc ? GameType.Foc : GameType.Eaw;
    }

    public static bool IsUpdatable(this ApplicationEnvironment modVerifyEnvironment)
    {
        return modVerifyEnvironment.IsUpdatable(out _);
    }

    public static bool IsUpdatable(this ApplicationEnvironment applicationEnvironment, out UpdatableApplicationEnvironment? updatableEnvironment)
    {
        updatableEnvironment = applicationEnvironment as UpdatableApplicationEnvironment;
        return updatableEnvironment is not null;
    }

    public static bool LaunchedWithoutArguments(this BaseModVerifyOptions options)
    {
        if (options is VerifyVerbOption verifyOptions)
            return verifyOptions.IsRunningWithoutArguments;
        return false;
    }
}