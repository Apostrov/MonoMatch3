using MonoGame.Extended;
using MonoGame.Extended.Tweening;

namespace MonoMatch3.Code.GameLogic.Components;

public struct GamePiece
{
    public Position BoardPosition;
    public Transform2 Transform;
    public float Radius;

    public struct Position
    {
        public int Row;
        public int Column;

        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
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
}