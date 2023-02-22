using Leopotam.EcsLite;
using MonoGame.Extended.Tweening;

namespace MonoMatch3.Code.GameLogic.Components;

public struct Selected
{
    public Tween AnimationTween;
    public bool IsUndo;
}

public struct SwapWith
{
}

public struct LastSwap
{
    public EcsPackedEntity Selected;
    public EcsPackedEntity Swapped;
}

public struct SwapWithotuMatch
{
}
