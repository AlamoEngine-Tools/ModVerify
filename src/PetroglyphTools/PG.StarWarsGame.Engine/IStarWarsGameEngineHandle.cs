using System;

namespace PG.StarWarsGame.Engine;

/// <summary>
/// An owned reference to a <see cref="IStarWarsGameEngine"/> that controls its lifetime.
/// Disposing this handle releases all resources held by the engine.
/// </summary>
public interface IStarWarsGameEngineHandle : IStarWarsGameEngine, IDisposable
{
}
