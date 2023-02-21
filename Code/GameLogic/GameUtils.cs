using System;

namespace MonoMatch3.Code.GameLogic;

public static class GameUtils
{
    private static readonly Random Random = new();

    public static Components.PieceType GetRandomType()
    {
        return (Components.PieceType)Random.Next(0, (int)Components.PieceType.COUNT);
    }
}