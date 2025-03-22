using System;
using System.Collections;
using System.Collections.Generic;
using AnakinRaW.CommonUtilities;
using AnakinRaW.CommonUtilities.Collections;
using PG.Commons.Collections;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.ALO.Files.Animations;

namespace PG.StarWarsGame.Engine.Rendering.Animations;

public sealed class AnimationCollection : DisposableObject, IEnumerable<IAloAnimationFile>
{
    public static readonly AnimationCollection Empty = new();

    private readonly ValueListDictionary<ModelAnimationType, IAloAnimationFile> _animations = new();
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

    public ReadOnlyFrugalList<IAloAnimationFile> GetAnimations(ModelAnimationType type)
    {
        return _animations.GetValues(type);
    }

    public bool TryGetAnimations(ModelAnimationType type, out ReadOnlyFrugalList<IAloAnimationFile> animations)
    {
        return _animations.TryGetValues(type, out animations);
    }

    public IAloAnimationFile GetAnimation(ModelAnimationType type, int subIndex)
    {
        if (subIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(subIndex), "subIndex cannot be negative.");
        var animations = GetAnimations(type);
        if (subIndex >= animations.Count)
            throw new ArgumentOutOfRangeException(nameof(subIndex), "subIndex cannot be larger than stored animations.");
        return animations[subIndex];
    }

    public bool TryGetAnimation(ModelAnimationType type, int subIndex, out IAloAnimationFile? animation)
    {
        animation = null;
        if (subIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(subIndex), "subIndex cannot be negative.");
        if (!TryGetAnimations(type, out var animations))
            return false;
        if (subIndex >= animations.Count)
            throw new ArgumentOutOfRangeException(nameof(subIndex), "subIndex cannot be larger than stored animations.");
        animation =  animations[subIndex];
        return true;
    }

    internal void AddAnimation(ModelAnimationType type, IAloAnimationFile animation, Crc32 crc)
    {
        _animations.Add(type, animation);
        _animationCrc.Add(type, crc);
    }

    protected override void DisposeResources()
    {
        foreach (var animation in _animations.Values) 
            animation.Dispose();
        _animationCrc.Clear();
        _animations.Clear();
    }

    public IEnumerator<IAloAnimationFile> GetEnumerator()
    {
        return _animations.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}