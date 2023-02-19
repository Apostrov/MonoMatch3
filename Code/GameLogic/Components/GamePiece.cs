using Microsoft.Xna.Framework;

namespace MonoMatch3.Code.GameLogic.Components;

public struct GamePiecePosition
{
    public int Row;
    public int Column;
    public Rectangle DrawnPosition;
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

public struct ClickedTag
{
}