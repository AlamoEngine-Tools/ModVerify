using System;
using PG.StarWarsGame.Engine.GameObjects;

namespace PG.StarWarsGame.Engine.Xml.Parsers;

internal sealed class GameObjectParsedEventArgs : EventArgs
{
    public bool Unique { get; }

    public GameObject GameObject { get; }

    internal GameObjectParsedEventArgs(GameObject gameObject, bool unique)
    {
        Unique = unique;
        GameObject = gameObject ?? throw new ArgumentNullException(nameof(gameObject));
    }
}