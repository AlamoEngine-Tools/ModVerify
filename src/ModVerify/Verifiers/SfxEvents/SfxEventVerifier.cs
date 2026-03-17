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
using PG.StarWarsGame.Engine.Audio.Sfx;

namespace AET.ModVerify.Verifiers.SfxEvents;

public sealed partial class SfxEventVerifier : NamedGameEntityVerifier<SfxEvent>
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

    public override IGameManager<SfxEvent> GameManager => GameEngine.SfxGameManager;
    public override string FriendlyName => "SFX Events";
    public override string EntityTypeName => "SFXEvent";

    public SfxEventVerifier(IStarWarsGameEngine gameEngine, GameVerifySettings settings, IServiceProvider serviceProvider) 
        : base( gameEngine, settings, serviceProvider)
    {
        _languageManager = serviceProvider.GetRequiredService<IGameLanguageManagerProvider>()
            .GetLanguageManager(Repository.EngineType);
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _audioFileVerifier = new AudioFileVerifier(this, gameEngine, settings, serviceProvider);
        _languagesToVerify = GetLanguagesToVerify();
    }

    protected override void VerifyEntity(SfxEvent entity, string[] context, double progress, CancellationToken token)
    {
        VerifyPresetRef(entity, context);
        VerifySamples(entity, context, token);
    }

    protected override void PostEntityVerify(CancellationToken token)
    {
        foreach (var sampleError in _audioFileVerifier.VerifyErrors) 
            AddError(sampleError);
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