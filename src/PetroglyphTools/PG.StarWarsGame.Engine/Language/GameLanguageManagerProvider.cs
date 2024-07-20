using System;

namespace PG.StarWarsGame.Engine.Language;

internal class GameLanguageManagerProvider(IServiceProvider serviceProvider) : IGameLanguageManagerProvider
{
    private readonly Lazy<IGameLanguageManager> _eawLanguageManager = new(() => new EawGameLanguageManager(serviceProvider));
    private readonly Lazy<IGameLanguageManager> _focLanguageManager = new(() => new FocGameLanguageManager(serviceProvider));

    public IGameLanguageManager GetLanguageManager(GameEngineType engine)
    {
        return engine switch
        {
            GameEngineType.Eaw => _eawLanguageManager.Value,
            GameEngineType.Foc => _focLanguageManager.Value,
            _ => throw new InvalidOperationException($"Engine '{engine}' not supported!")
        };
    }
}