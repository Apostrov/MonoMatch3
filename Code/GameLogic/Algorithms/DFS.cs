using System.Collections.Generic;
using Leopotam.EcsLite;
using MonoMatch3.Code.GameLogic.Components;

namespace MonoMatch3.Code.GameLogic.Algorithms;

public abstract class DFS
{
    private readonly EcsWorld _world;
    private readonly EcsPool<GamePiece> _piecePool;

    protected DFS(EcsPool<GamePiece> piecePool, EcsWorld world)
    {
        _world = world;
        _piecePool = piecePool;
    }

    protected abstract bool IsCorrect(int startPieceEntity, int currentPieceEntity);

    public delegate void GetNextPosition(PiecePosition position, ref Stack<PiecePosition> toAdd);

    public List<int> Solve(EcsPackedEntity startBlock, EcsPackedEntity[,] board,
        GetNextPosition nextPosition)
    {
        if (!startBlock.Unpack(_world, out var startBlockEntity) || !IsCorrect(startBlockEntity, startBlockEntity))
            return new List<int>();

        var collectedEntities = new List<int>();
        var dfsStack = new Stack<PiecePosition>();
        var discovered = new bool[board.GetLength(0), board.GetLength(1)];
        ref var piece = ref _piecePool.Get(startBlockEntity);
        dfsStack.Push(piece.BoardPosition);
        while (dfsStack.Count > 0)
        {
            var position = dfsStack.Pop();
            if (position.Row < 0 || position.Column < 0 ||
                position.Row >= board.GetLength(0) || position.Column >= board.GetLength(1) ||
                discovered[position.Row, position.Column])
                continue;

            var entityPacked = board[position.Row, position.Column];
            if (!entityPacked.Unpack(_world, out var entity) || !IsCorrect(startBlockEntity, entity))
                continue;

            collectedEntities.Add(entity);
            discovered[position.Row, position.Column] = true;
            nextPosition(position, ref dfsStack);
        }

        return collectedEntities;
    }
}