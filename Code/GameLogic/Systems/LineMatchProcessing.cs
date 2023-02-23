using System.Collections.Generic;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class LineMatchProcessing : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.Line, Components.GamePiece, Components.BonusMatch>> _lineMatch =
        default;

    private readonly EcsFilterInject<Inc<Components.GameBoard>> _gameBoard = default;

    private readonly EcsPoolInject<Components.DestroyPiece> _destroyPool = default;
    private readonly EcsPoolInject<Components.LineDestroyer> _destroyer = default;

    private readonly EcsWorldInject _world = default;

    public void Run(IEcsSystems systems)
    {
        if (_lineMatch.Value.GetEntitiesCount() < 1)
            return;

        foreach (var lineMatchEntity in _lineMatch.Value)
        {
            ref var line = ref _lineMatch.Pools.Inc1.Get(lineMatchEntity);
            ref var linePiece = ref _lineMatch.Pools.Inc2.Get(lineMatchEntity);
            foreach (var gameBoardEntity in _gameBoard.Value)
            {
                ref var gameBoard = ref _gameBoard.Pools.Inc1.Get(gameBoardEntity);
                switch (line.Type)
                {
                    case LineType.Column:
                        LineDestroyer(linePiece, gameBoard.Board, LeftMover());
                        LineDestroyer(linePiece, gameBoard.Board, RightMover());
                        break;
                    case LineType.Row:
                        LineDestroyer(linePiece, gameBoard.Board, UpMover());
                        LineDestroyer(linePiece, gameBoard.Board, DownMover());
                        break;
                }
            }

            if (!_destroyPool.Value.Has(lineMatchEntity))
                _destroyPool.Value.Add(lineMatchEntity).WaitTime = GameConfig.DESTROY_ANIMATION_TIME;
            _lineMatch.Pools.Inc3.Del(lineMatchEntity);
        }
    }

    private void LineDestroyer(Components.GamePiece startBlock, EcsPackedEntity[,] board,
        GameUtils.GetNextPosition nextPosition)
    {
        var toDestroy = new List<(Vector2, EcsPackedEntity)>();
        var nextPiece = new Stack<PiecePosition>();
        var discovered = new bool[board.GetLength(0), board.GetLength(1)];
        nextPiece.Push(startBlock.BoardPosition);

        while (nextPiece.Count > 0)
        {
            var position = nextPiece.Pop();
            if (position.Row < 0 || position.Column < 0 ||
                position.Row >= board.GetLength(0) || position.Column >= board.GetLength(1) ||
                discovered[position.Row, position.Column])
                continue;

            var entityPacked = board[position.Row, position.Column];
            if (!entityPacked.Unpack(_world.Value, out var destroyEntity) || _destroyPool.Value.Has(destroyEntity))
                continue;

            var drawPosition = _lineMatch.Pools.Inc2.Get(destroyEntity).Transform.Position;
            toDestroy.Add((drawPosition, entityPacked));
            discovered[position.Row, position.Column] = true;
            nextPosition(position, ref nextPiece);
        }

        SpawnDestroyer(startBlock.Transform.Position, toDestroy);
    }

    private void SpawnDestroyer(Vector2 spawnPosition, List<(Vector2, EcsPackedEntity)> points)
    {
        ref var destroyer = ref _destroyer.Value.Add(_world.Value.NewEntity());
        destroyer.FlyPosition = spawnPosition;
        destroyer.FlyPoints = new Queue<(Vector2, EcsPackedEntity)>(points);
        destroyer.Transform = new Transform2(spawnPosition);
    }

    private static GameUtils.GetNextPosition LeftMover()
    {
        return (PiecePosition piecePosition, ref Stack<PiecePosition> add) =>
        {
            add.Push(new PiecePosition(piecePosition.Row - 1, piecePosition.Column));
        };
    }

    private static GameUtils.GetNextPosition RightMover()
    {
        return (PiecePosition piecePosition, ref Stack<PiecePosition> add) =>
        {
            add.Push(new PiecePosition(piecePosition.Row + 1, piecePosition.Column));
        };
    }

    private static GameUtils.GetNextPosition UpMover()
    {
        return (PiecePosition piecePosition, ref Stack<PiecePosition> add) =>
        {
            add.Push(new PiecePosition(piecePosition.Row, piecePosition.Column - 1));
        };
    }

    private static GameUtils.GetNextPosition DownMover()
    {
        return (PiecePosition piecePosition, ref Stack<PiecePosition> add) =>
        {
            add.Push(new PiecePosition(piecePosition.Row, piecePosition.Column + 1));
        };
    }
}