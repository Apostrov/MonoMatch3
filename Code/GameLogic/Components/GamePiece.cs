using MonoGame.Extended;
using MonoGame.Extended.Tweening;

namespace MonoMatch3.Code.GameLogic.Components;

public struct GamePiece
{
    public PiecePosition BoardPosition;
    public Transform2 Transform;
    public float Radius;
}

public enum PieceType
{
    Yellow = 0,
    Blue,
    Red,
    Green,
    Purple,
    COUNT
}

public struct GamePieceType
{
    public PieceType Type;
}

public struct Selected
{
    public Tween AnimationTween;
}

public struct SwapWith
{
}

public struct DestroyPiece
{
    public float WaitTime;
}