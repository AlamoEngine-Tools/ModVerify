using AET.ModVerifyTool.Settings;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure;

namespace AET.ModVerifyTool.ModSelectors;

internal interface IModSelector
{
    GameLocations? Select(
        GameInstallationsSettings settings,
        out IPhysicalPlayableObject? targetObject, 
        out GameEngineType? actualEngineType);
}