using System.Collections.Generic;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;

namespace MonoMatch3.Code.GameLogic.Systems;

public class Match3Solver : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.GameBoard>> _gameBoard = default;
    private readonly EcsFilterInject<Inc<Components.SolvePieceMatch>> _solveMatch = default;
    
    private readonly EcsPoolInject<Components.SolvePieceMatch> _solveMatchPool = default;
    private readonly EcsPoolInject<Components.GamePiece> _piecePool = default;
    private readonly EcsPoolInject<Components.GamePieceType> _pieceTypePool = default;
    private readonly EcsPoolInject<Components.DestroyPiece> _destroyPool = default;

    private readonly EcsSharedInject<SharedData> _shared = default;
    private readonly EcsWorldInject _world = default;

    public void Run(IEcsSystems systems)
    {
        if (_solveMatch.Value.GetEntitiesCount() < 1)
            return;

        foreach (var boardEntity in _gameBoard.Value)
        {
            ref var board = ref _gameBoard.Pools.Inc1.Get(boardEntity);
            foreach (var solveEntity in _solveMatch.Value)
            {
                var toDestroy = DfsSolver(_solveMatchPool.Value.Get(solveEntity).StartPiece, board.Board);
                if (toDestroy.Count < GameConfig.MATCH_COUNT)
                    continue;

                foreach (var destroyPack in toDestroy)
                {
                    if (!destroyPack.Unpack(_world.Value, out var entity))
                        continue;

                    ref var piece = ref _piecePool.Value.Get(entity);
                    _shared.Value.Tweener.TweenTo(target: piece.Transform,
                        expression: t => t.Scale,
                        toValue: Vector2.Zero,
                        duration: GameConfig.DESTROY_ANIMATION_TIME);
                    _destroyPool.Value.Add(entity);
                }

                _solveMatchPool.Value.Del(solveEntity);
            }
        }
    }

    private List<EcsPackedEntity> DfsSolver(EcsPackedEntity startBlock, EcsPackedEntity[,] board)
    {
        if (!startBlock.Unpack(_world.Value, out var startBlockEntity) || _destroyPool.Value.Has(startBlockEntity))
            return new List<EcsPackedEntity>();

        var toDestroy = new List<EcsPackedEntity>();
        var dfsStack = new Stack<(int row, int column)>();
        var discovered = new bool[board.GetLength(0), board.GetLength(1)];
        var color = _pieceTypePool.Value.Get(startBlockEntity).Type;
        ref var piece = ref _piecePool.Value.Get(startBlockEntity);
        dfsStack.Push((piece.Row, piece.Column));
        while (dfsStack.Count > 0)
        {
            var position = dfsStack.Pop();
            if (position.row < 0 || position.column < 0 ||
                position.row >= board.GetLength(0) || position.column >= board.GetLength(1) ||
                discovered[position.row, position.column])
                continue;

            var entityPacked = board[position.row, position.column];
            if (!entityPacked.Unpack(_world.Value, out var entity) || _destroyPool.Value.Has(entity) ||
                _pieceTypePool.Value.Get(entity).Type != color)
                continue;

            toDestroy.Add(entityPacked);
            discovered[position.row, position.column] = true;
            dfsStack.Push((position.row + 1, position.column));
            dfsStack.Push((position.row, position.column + 1));
            dfsStack.Push((position.row - 1, position.column));
            dfsStack.Push((position.row, position.column - 1));
        }

        return toDestroy;
    }
}