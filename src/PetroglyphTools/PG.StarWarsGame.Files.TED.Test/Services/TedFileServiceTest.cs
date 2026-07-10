using System.IO;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.Hashing;
using AnakinRaW.CommonUtilities.Testing;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons;
using PG.StarWarsGame.Files.TED.Services;
using Testably.Abstractions;
using Xunit;

namespace PG.StarWarsGame.Files.TED.Test.Services;

public class TedFileServiceTest : TestBaseWithFileSystem
{
    private readonly TedFileService _service;

    protected override IFileSystem CreateFileSystem()
    {
        return new RealFileSystem();
    }

    public TedFileServiceTest()
    {
        _service = new TedFileService(ServiceProvider);
    }

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton<IHashingService>(sp => new HashingService(sp));
        PetroglyphCommons.ContributeServices(serviceCollection);
        serviceCollection.SupportTED();
    }

    [Fact]
    public void Foo()
    {
        const string path = @"C:\Program Files (x86)\Steam\steamapps\workshop\content\32470\1129810972\Data\Art\Maps\_land_planet_felucia_00.ted";
        using var tedFs = FileSystem.FileStream.New(path, FileMode.Open);
        using var dest = FileSystem.FileStream.New("c:/test/map.ted", FileMode.Create);
        _service.RemoveMapPreview(tedFs, dest, true, out var bytes);

        if (bytes is not null)
        {
            using var img = FileSystem.FileStream.New("c:/test/img.dds", FileMode.Create);
            using var streamWriter = new BinaryWriter(img);
            streamWriter.Write(bytes);
        }
       
    }
}