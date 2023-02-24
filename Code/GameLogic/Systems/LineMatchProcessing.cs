using System.Collections.Generic;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class LineMatchProcessing : IEcsInitSystem, IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.Line, Components.GamePiece, Components.BonusMatch>> _lineMatch =
        default;

    private readonly EcsFilterInject<Inc<Components.GameBoard>> _gameBoard = default;

    private readonly EcsPoolInject<Components.DestroyPiece> _destroyPool = default;
    private readonly EcsPoolInject<Components.LineDestroyer> _destroyer = default;

    private readonly EcsWorldInject _world = default;

    private Algorithms.DFS _dfs;

    public void Init(IEcsSystems systems)
    {
        _dfs = new Algorithms.SimpleDFS(_lineMatch.Pools.Inc2, _world.Value);
    }

    public void Run(IEcsSystems systems)
    {
        foreach (var lineMatchEntity in _lineMatch.Value)
        {
            ref var line = ref _lineMatch.Pools.Inc1.Get(lineMatchEntity);
            foreach (var gameBoardEntity in _gameBoard.Value)
            {
                ref var gameBoard = ref _gameBoard.Pools.Inc1.Get(gameBoardEntity);
                switch (line.Type)
                {
                    case LineType.Row:
                        LineDestroyer(lineMatchEntity, gameBoard.Board, LeftMover(), LineDestroyerType.Left);
                        LineDestroyer(lineMatchEntity, gameBoard.Board, RightMover(), LineDestroyerType.Right);
                        break;
                    case LineType.Column:
                        LineDestroyer(lineMatchEntity, gameBoard.Board, UpMover(), LineDestroyerType.Up);
                        LineDestroyer(lineMatchEntity, gameBoard.Board, DownMover(), LineDestroyerType.Down);
                        break;
                }
            }

            if (!_destroyPool.Value.Has(lineMatchEntity))
                _destroyPool.Value.Add(lineMatchEntity).WaitTime = GameConfig.DESTROY_ANIMATION_TIME;
            _lineMatch.Pools.Inc3.Del(lineMatchEntity);
        }
    }

    private void LineDestroyer(int startBlockEntity, EcsPackedEntity[,] board,
        Algorithms.DFS.GetNextPosition nextPosition, LineDestroyerType type)
    {
        var entities = _dfs.Solve(_world.Value.PackEntity(startBlockEntity), board, nextPosition);
        (Vector2, EcsPackedEntity)[] points = new (Vector2, EcsPackedEntity)[entities.Count];
        for (int i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];
            var drawPosition = _lineMatch.Pools.Inc2.Get(entity).Transform.Position;
            points[i] = (drawPosition, _world.Value.PackEntity(entity));
        }

        ref var piece = ref _lineMatch.Pools.Inc2.Get(startBlockEntity);
        SpawnDestroyer(piece.Transform.Position, points, type);
    }

    private void SpawnDestroyer(Vector2 spawnPosition, (Vector2, EcsPackedEntity)[] points, LineDestroyerType type)
    {
        ref var destroyer = ref _destroyer.Value.Add(_world.Value.NewEntity());
        destroyer.Type = type;
        destroyer.FlyPosition = spawnPosition;
        destroyer.FlyPoints = new Queue<(Vector2, EcsPackedEntity)>(points);
        destroyer.Transform = new Transform2(spawnPosition);
    }

    private Algorithms.DFS.GetNextPosition LeftMover()
    {
        return (PiecePosition piecePosition, ref Stack<PiecePosition> add) =>
        {
            add.Push(new PiecePosition(piecePosition.Row - 1, piecePosition.Column));
        };
    }

    private Algorithms.DFS.GetNextPosition RightMover()
    {
        return (PiecePosition piecePosition, ref Stack<PiecePosition> add) =>
        {
            add.Push(new PiecePosition(piecePosition.Row + 1, piecePosition.Column));
        };
    }

    private Algorithms.DFS.GetNextPosition UpMover()
    {
        return (PiecePosition piecePosition, ref Stack<PiecePosition> add) =>
        {
            add.Push(new PiecePosition(piecePosition.Row, piecePosition.Column - 1));
        };
    }

    private Algorithms.DFS.GetNextPosition DownMover()
    {
        return (PiecePosition piecePosition, ref Stack<PiecePosition> add) =>
        {
            add.Push(new PiecePosition(piecePosition.Row, piecePosition.Column + 1));
        };
    }
}