using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Components;

public struct GamePiece
{
    public int Row;
    public int Column;
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

public struct Clicked
{
}