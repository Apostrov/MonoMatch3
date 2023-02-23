using System;
using System.Collections.Generic;

namespace MonoMatch3.Code.GameLogic;

public static class GameUtils
{
    private static readonly Random Random = new();

    public static PieceType GetRandomType()
    {
        return (PieceType)Random.Next(0, (int)PieceType.COUNT);
    }

    public delegate void GetNextPosition(PiecePosition position, ref Stack<PiecePosition> toAdd);

    public static GetNextPosition RowMover()
    {
        return (PiecePosition piecePosition, ref Stack<PiecePosition> add) =>
        {
            add.Push(new PiecePosition(piecePosition.Row + 1, piecePosition.Column));
            add.Push(new PiecePosition(piecePosition.Row - 1, piecePosition.Column));
        };
    }

    public static GetNextPosition ColumnMover()
    {
        return (PiecePosition piecePosition, ref Stack<PiecePosition> add) =>
        {
            add.Push(new PiecePosition(piecePosition.Row, piecePosition.Column + 1));
            add.Push(new PiecePosition(piecePosition.Row, piecePosition.Column - 1));
        };
    }
}