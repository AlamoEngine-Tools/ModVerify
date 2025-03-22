using System;
using System.Collections.Generic;
using AET.ModVerify;
using AET.ModVerify.Settings;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Database;

namespace AET.ModVerifyTool;

internal class ModVerifyPipeline(
    GameEngineType targetType,
    GameLocations gameLocations,
    GameVerifySettings settings,
    IServiceProvider serviceProvider)
    : VerifyGamePipeline(targetType, gameLocations, settings, serviceProvider)
{
    protected override IEnumerable<IGameVerifier> CreateVerificationSteps(IGameDatabase database)
    {
        var verifyProvider = ServiceProvider.GetRequiredService<IVerificationProvider>();
        return verifyProvider.GetAllDefaultVerifiers(database, Settings);
    }
}