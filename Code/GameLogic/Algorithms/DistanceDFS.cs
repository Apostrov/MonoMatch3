using System;
using Leopotam.EcsLite;
using MonoMatch3.Code.GameLogic.Components;

namespace MonoMatch3.Code.GameLogic.Algorithms;

public class DistanceDFS : DFS
{
    private readonly int _maxDistance;

    public DistanceDFS(EcsPool<GamePiece> piecePool, EcsWorld world, int maxDistance) : base(piecePool, world)
    {
        _maxDistance = maxDistance;
    }

    protected override bool IsCorrect(int startPieceEntity, int currentPieceEntity)
    {
        return PiecePool.Has(startPieceEntity) && PiecePool.Has(currentPieceEntity) &&
               IsInDistance(PiecePool.Get(startPieceEntity).BoardPosition,
                   PiecePool.Get(currentPieceEntity).BoardPosition);
    }

    private bool IsInDistance(PiecePosition start, PiecePosition current)
    {
        return Math.Abs(start.Row - current.Row) <= _maxDistance &&
               Math.Abs(start.Column - current.Column) <= _maxDistance;
    }
}