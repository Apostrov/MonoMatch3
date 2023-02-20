using System.Collections.Generic;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;

namespace MonoMatch3.Code.GameLogic.Systems;

public class Match3Solver : IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter _gameBoard;
    private EcsFilter _solveMatch;

    private EcsPool<Components.GameBoard> _gameBoardPool;
    private EcsPool<Components.SolvePieceMatch> _solveMatchPool;
    private EcsPool<Components.GamePiece> _piecePool;
    private EcsPool<Components.GamePieceType> _pieceTypePool;
    private EcsPool<Components.DestroyPiece> _destroyPool;

    private EcsWorld _world;
    private SharedData _shared;

    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();
        _shared = systems.GetShared<SharedData>();

        _gameBoard = _world.Filter<Components.GameBoard>().End();
        _solveMatch = _world.Filter<Components.SolvePieceMatch>().End();

        _gameBoardPool = _world.GetPool<Components.GameBoard>();
        _solveMatchPool = _world.GetPool<Components.SolvePieceMatch>();
        _piecePool = _world.GetPool<Components.GamePiece>();
        _pieceTypePool = _world.GetPool<Components.GamePieceType>();
        _destroyPool = _world.GetPool<Components.DestroyPiece>();
    }

    public void Run(IEcsSystems systems)
    {
        if (_solveMatch.GetEntitiesCount() < 1)
            return;

        foreach (var boardEntity in _gameBoard)
        {
            ref var board = ref _gameBoardPool.Get(boardEntity);
            foreach (var solveEntity in _solveMatch)
            {
                var toDestroy = DfsSolver(_solveMatchPool.Get(solveEntity).StartPiece, board.Board);
                if (toDestroy.Count < GameConfig.MATCH_COUNT)
                    continue;

                foreach (var destroyPack in toDestroy)
                {
                    if (!destroyPack.Unpack(_world, out var entity))
                        continue;

                    ref var piece = ref _piecePool.Get(entity);
                    _shared.Tweener.TweenTo(target: piece.Transform,
                        expression: t => t.Scale,
                        toValue: Vector2.Zero,
                        duration: GameConfig.DESTROY_ANIMATION_TIME);
                    _destroyPool.Add(entity);
                }

                _solveMatchPool.Del(solveEntity);
            }
        }
    }

    private List<EcsPackedEntity> DfsSolver(EcsPackedEntity startBlock, EcsPackedEntity[,] board)
    {
        if (!startBlock.Unpack(_world, out var startBlockEntity) || _destroyPool.Has(startBlockEntity))
            return new List<EcsPackedEntity>();

        var toDestroy = new List<EcsPackedEntity>();
        var dfsStack = new Stack<(int row, int column)>();
        var discovered = new bool[board.GetLength(0), board.GetLength(1)];
        var color = _pieceTypePool.Get(startBlockEntity).Type;
        ref var piece = ref _piecePool.Get(startBlockEntity);
        dfsStack.Push((piece.Row, piece.Column));
        while (dfsStack.Count > 0)
        {
            var position = dfsStack.Pop();
            if (position.row < 0 || position.column < 0 ||
                position.row >= board.GetLength(0) || position.column >= board.GetLength(1) ||
                discovered[position.row, position.column])
                continue;

            var entityPacked = board[position.row, position.column];
            if (!entityPacked.Unpack(_world, out var entity) || _destroyPool.Has(entity) ||
                _pieceTypePool.Get(entity).Type != color)
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