﻿using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure.Games;

namespace AET.ModVerifyTool;

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
}