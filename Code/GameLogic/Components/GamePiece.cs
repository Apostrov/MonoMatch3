using MonoGame.Extended;
using MonoGame.Extended.Tweening;

namespace MonoMatch3.Code.GameLogic.Components;

public struct GamePiece
{
    public PiecePosition BoardPosition;
    public Transform2 Transform;
    public float Radius;
}

public struct GamePieceType
{
    public PieceType Type;
}

public struct DestroyPiece
{
    public Tween Animation;
    public float WaitTime;
}