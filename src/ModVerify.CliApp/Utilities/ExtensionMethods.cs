using System.Diagnostics.CodeAnalysis;
using AET.ModVerify.App.Settings.CommandLine;
using AnakinRaW.ApplicationBase.Environment;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure.Games;

namespace AET.ModVerify.App.Utilities;

internal static class ExtensionMethods
{
    extension(GameEngineType type)
    {
        public GameType FromEngineType()
        {
            return (GameType)(int)type;
        }

        public GameEngineType Opposite()
        {
            return (GameEngineType)((int)type ^ 1);
        }
    }

    public static GameEngineType ToEngineType(this GameType type)
    {
        return (GameEngineType)(int)type;
    }
    extension(ApplicationEnvironment modVerifyEnvironment)
    {
        public bool IsUpdatable()
        {
            return modVerifyEnvironment.IsUpdatable(out _);
        }

        public bool IsUpdatable([NotNullWhen(true)] out UpdatableApplicationEnvironment? updatableEnvironment)
        {
            updatableEnvironment = modVerifyEnvironment as UpdatableApplicationEnvironment;
            return updatableEnvironment is not null;
        }
    }

    public static bool LaunchedWithoutArguments(this BaseModVerifyOptions options)
    {
        if (options is VerifyVerbOption verifyOptions)
            return verifyOptions.IsRunningWithoutArguments;
        return false;
    }
}