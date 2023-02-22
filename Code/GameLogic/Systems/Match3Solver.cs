﻿using System.Collections.Generic;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class Match3Solver : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.GameBoard>> _gameBoard = default;
    private readonly EcsFilterInject<Inc<Components.SolvePieceMatch>> _solveMatch = default;

    private readonly EcsPoolInject<Components.GamePiece> _piecePool = default;
    private readonly EcsPoolInject<Components.GamePieceType> _pieceTypePool = default;
    private readonly EcsPoolInject<Components.DestroyPiece> _destroyPool = default;
    private readonly EcsPoolInject<Components.RearrangeBoard> _rearrangePool = default;
    private readonly EcsPoolInject<Components.LineSpawn> _lineSpawnPool = default;

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
                ref var solveMatch = ref _solveMatch.Pools.Inc1.Get(solveEntity);
                solveMatch.WaitTime -= _shared.Value.GameTime.GetElapsedSeconds();
                if (solveMatch.WaitTime > 0.0f)
                    continue;

                var rowToDestroy = RowDfsSolver(solveMatch.StartPiece, board.Board);
                var columnToDestroy = ColumnDfsSolver(solveMatch.StartPiece, board.Board);
                int destroyed = DestroyLine(rowToDestroy);
                destroyed += DestroyLine(columnToDestroy);
                if (destroyed > 0)
                {
                    if (destroyed >= GameConfig.LINE_BONUS_COUNT)
                    {
                        if (solveMatch.StartPiece.Unpack(_world.Value, out var entity))
                        {
                            _lineSpawnPool.Value.Add(_world.Value.NewEntity()) = new Components.LineSpawn
                            {
                                LinePosition = _piecePool.Value.Get(entity).BoardPosition,
                                WaitTime = GameConfig.DESTROY_ANIMATION_TIME
                            };
                        }
                    }

                    _rearrangePool.Value.Add(_world.Value.NewEntity()).WaitTime = GameConfig.DESTROY_ANIMATION_TIME;
                }
                else
                {
                    solveMatch.OnDontMatchCallback?.Invoke();
                }

                _solveMatch.Pools.Inc1.Del(solveEntity);
            }
        }
    }

    private int DestroyLine(List<EcsPackedEntity> line)
    {
        if (line.Count < GameConfig.MATCH_COUNT)
            return 0;

        int destroyed = 0;
        foreach (var destroyPack in line)
        {
            if (!destroyPack.Unpack(_world.Value, out var entity) || _destroyPool.Value.Has(entity))
                continue;

            ref var piece = ref _piecePool.Value.Get(entity);
            _shared.Value.Tweener.TweenTo(target: piece.Transform,
                expression: t => t.Scale,
                toValue: Vector2.Zero,
                duration: GameConfig.DESTROY_ANIMATION_TIME);
            _destroyPool.Value.Add(entity).WaitTime = GameConfig.DESTROY_ANIMATION_TIME;
            destroyed++;
        }

        return destroyed;
    }


    private List<EcsPackedEntity> RowDfsSolver(EcsPackedEntity startBlock, EcsPackedEntity[,] board)
    {
        return DfsSolver(startBlock, board,
            (PiecePosition position, ref Stack<PiecePosition> add) =>
            {
                add.Push(new PiecePosition(position.Row + 1, position.Column));
                add.Push(new PiecePosition(position.Row - 1, position.Column));
            });
    }

    private List<EcsPackedEntity> ColumnDfsSolver(EcsPackedEntity startBlock, EcsPackedEntity[,] board)
    {
        return DfsSolver(startBlock, board,
            (PiecePosition position, ref Stack<PiecePosition> add) =>
            {
                add.Push(new PiecePosition(position.Row, position.Column + 1));
                add.Push(new PiecePosition(position.Row, position.Column - 1));
            });
    }

    private List<EcsPackedEntity> DfsSolver(EcsPackedEntity startBlock, EcsPackedEntity[,] board,
        GetNextPosition nextPosition)
    {
        if (!startBlock.Unpack(_world.Value, out var startBlockEntity) ||
            _destroyPool.Value.Has(startBlockEntity) ||
            !_pieceTypePool.Value.Has(startBlockEntity))
            return new List<EcsPackedEntity>();

        var toDestroy = new List<EcsPackedEntity>();
        var dfsStack = new Stack<PiecePosition>();
        var discovered = new bool[board.GetLength(0), board.GetLength(1)];
        var color = _pieceTypePool.Value.Get(startBlockEntity).Type;
        ref var piece = ref _piecePool.Value.Get(startBlockEntity);
        dfsStack.Push(piece.BoardPosition);
        while (dfsStack.Count > 0)
        {
            var position = dfsStack.Pop();
            if (position.Row < 0 || position.Column < 0 ||
                position.Row >= board.GetLength(0) || position.Column >= board.GetLength(1) ||
                discovered[position.Row, position.Column])
                continue;

            var entityPacked = board[position.Row, position.Column];
            if (!entityPacked.Unpack(_world.Value, out var entity) || _destroyPool.Value.Has(entity) ||
                !_pieceTypePool.Value.Has(entity) || _pieceTypePool.Value.Get(entity).Type != color)
                continue;

            toDestroy.Add(entityPacked);
            discovered[position.Row, position.Column] = true;
            nextPosition(position, ref dfsStack);
        }

        return toDestroy;
    }

    private delegate void GetNextPosition(PiecePosition position, ref Stack<PiecePosition> toAdd);
}