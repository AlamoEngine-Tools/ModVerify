using AET.SteamAbstraction;
using AnakinRaW.CommonUtilities.Hashing;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons;
using PG.StarWarsGame.Infrastructure;
using PG.StarWarsGame.Infrastructure.Clients.Steam;
using System;
using System.IO.Abstractions;
using Testably.Abstractions.Testing;

namespace ModVerify.CliApp.Test;

public abstract class CommonTestBase
{
    protected readonly MockFileSystem FileSystem = new();
    protected readonly IServiceProvider ServiceProvider;

    protected CommonTestBase()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton<IHashingService>(sp => new HashingService(sp));
        sc.AddSingleton<IFileSystem>(FileSystem);
        PetroglyphCommons.ContributeServices(sc);
        PetroglyphGameInfrastructure.InitializeServices(sc);
        SteamAbstractionLayer.InitializeServices(sc);
        SteamPetroglyphStarWarsGameClients.InitializeServices(sc);
        // ReSharper disable once VirtualMemberCallInConstructor
        SetupServices(sc);
        ServiceProvider = sc.BuildServiceProvider();
    }

    protected virtual void SetupServices(ServiceCollection serviceCollection)
    {
    }
}