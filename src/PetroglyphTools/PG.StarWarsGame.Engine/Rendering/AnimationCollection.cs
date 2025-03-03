using System;
using AnakinRaW.CommonUtilities;
using AnakinRaW.CommonUtilities.Collections;
using PG.Commons.Collections;
using PG.Commons.Hashing;

namespace PG.StarWarsGame.Engine.Rendering;

public sealed class AnimationCollection : DisposableObject
{
    private readonly ValueListDictionary<ModelAnimationType, object> _animations = new();
    private readonly ValueListDictionary<ModelAnimationType, Crc32> _animationCrc = new();

    public int Cout => _animations.Count;

    public Crc32 GetAnimationCrc(ModelAnimationType type, int subIndex)
    {
        if (subIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(subIndex), "subIndex cannot be negative.");
        var checksumsForType = _animationCrc.GetValues(type);
        if (subIndex >= checksumsForType.Count)
            throw new ArgumentOutOfRangeException(nameof(subIndex), "subIndex cannot be larger than stored animations.");
        return checksumsForType[subIndex];
    }

    public ReadOnlyFrugalList<object> GetAnimations(ModelAnimationType type)
    {
        return _animations.GetValues(type);
    }

    public bool TryGetAnimations(ModelAnimationType type, out ReadOnlyFrugalList<object> animations)
    {
        return _animations.TryGetValues(type, out animations);
    }

    public object GetAnimation(ModelAnimationType type, int subIndex)
    {
        if (subIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(subIndex), "subIndex cannot be negative.");
        var animations = GetAnimations(type);
        if (subIndex >= animations.Count)
            throw new ArgumentOutOfRangeException(nameof(subIndex), "subIndex cannot be larger than stored animations.");
        return animations[subIndex];
    }

    internal void AddAnimation(ModelAnimationType type, object animation, Crc32 crc)
    {
        _animations.Add(type, animation);
        _animationCrc.Add(type, crc);
    }

    protected override void DisposeResources()
    {
        foreach (var animation in _animations)
        {
            if (animation.Value is IDisposable disposable)
                disposable.Dispose();
        }   
        //_animationCrc.Clear();
        //_animations.Clear();
    }
}