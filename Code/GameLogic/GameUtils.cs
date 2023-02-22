using System;

namespace MonoMatch3.Code.GameLogic;

public static class GameUtils
{
    private static readonly Random Random = new();

    public static PieceType GetRandomType()
    {
        return (PieceType)Random.Next(0, (int)PieceType.COUNT);
    }
}