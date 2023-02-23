using System;
using System.Collections.Generic;

namespace MonoMatch3.Code.GameLogic;

public static class GameUtils
{
    private static readonly Random Random = new();

    public static PieceType GetRandomPiece()
    {
        return (PieceType)Random.Next(0, (int)PieceType.COUNT);
    }

    public static LineType GetRandomLine()
    {
        return (LineType)Random.Next(0, (int)LineType.COUNT);
    }

    public delegate void GetNextPosition(PiecePosition position, ref Stack<PiecePosition> toAdd);
}