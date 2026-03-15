using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers.Commons;
using AnakinRaW.CommonUtilities.FileSystem.Normalization;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Localization;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading;

namespace AET.ModVerify.Verifiers.SfxEvents;

public sealed partial class SfxEventVerifier : GameVerifier
{
    private static readonly PathNormalizeOptions SampleNormalizerOptions = new()
    {
        UnifyCase = UnifyCasingKind.UpperCaseForce,
        UnifySeparatorKind = DirectorySeparatorKind.Windows,
        UnifyDirectorySeparators = true
    };

    private readonly IGameLanguageManager _languageManager;
    private readonly IFileSystem _fileSystem;
    private readonly AudioFileVerifier _audioFileVerifier;
    private readonly IReadOnlyCollection<LanguageType> _languagesToVerify;

    public override string FriendlyName => "SFX Events";

    public SfxEventVerifier(IStarWarsGameEngine gameEngine, GameVerifySettings settings, IServiceProvider serviceProvider) 
        : base(null, gameEngine, settings, serviceProvider)
    {
        _languageManager = serviceProvider.GetRequiredService<IGameLanguageManagerProvider>()
            .GetLanguageManager(Repository.EngineType);
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _audioFileVerifier = new AudioFileVerifier(this, gameEngine, settings, serviceProvider);
        _languagesToVerify = GetLanguagesToVerify();
    }

    public override void Verify(CancellationToken token)
    {
        VerifyDuplicates(token);

        var numEvents = GameEngine.SfxGameManager.Entries.Count;
        double counter = 0;
        foreach (var sfxEvent in GameEngine.SfxGameManager.Entries)
        {
            OnProgress(++counter / numEvents, $"SFX Event - '{sfxEvent.Name}'");

            VerifyPresetRef(sfxEvent, token);
            VerifySamples(sfxEvent, token);
        }
    }

    private IReadOnlyCollection<LanguageType> GetLanguagesToVerify()
    {
        switch (Settings.LocalizationOption)
        {
            case VerifyLocalizationOption.English:
                return [LanguageType.English];
            case VerifyLocalizationOption.CurrentSystem:
                return [_languageManager.GetLanguagesFromUser()];
            case VerifyLocalizationOption.AllInstalled:
                return [..GameEngine.InstalledLanguages];
            case VerifyLocalizationOption.All:
                return [.._languageManager.SupportedLanguages];
            default:
                throw new NotSupportedException($"{Settings.LocalizationOption} is not supported");
        }
    }
}