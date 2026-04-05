using System;
using PG.StarWarsGame.Engine.GameObjects;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal sealed class GameObjectParsedEventArgs : EventArgs
{
    public bool Unique { get; }

    public GameObjectType GameObjectType { get; }

    internal GameObjectParsedEventArgs(GameObjectType gameObjectType, bool unique)
    {
        Unique = unique;
        GameObjectType = gameObjectType ?? throw new ArgumentNullException(nameof(gameObjectType));
    }
}