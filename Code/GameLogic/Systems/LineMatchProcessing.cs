using System.Collections.Generic;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace MonoMatch3.Code.GameLogic.Systems;

public class LineMatchProcessing : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.Line, Components.GamePiece, Components.BonusMatch>> _lineMatch =
        default;

    private readonly EcsFilterInject<Inc<Components.GameBoard>> _gameBoard = default;

    private readonly EcsPoolInject<Components.DestroyPiece> _destroyPool = default;

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
                    case LineType.Row:
                        LineSolver(linePiece.BoardPosition, gameBoard.Board, GameUtils.ColumnMover());
                        break;
                    case LineType.Column:
                        LineSolver(linePiece.BoardPosition, gameBoard.Board, GameUtils.RowMover());
                        break;
                }
            }

            if (!_destroyPool.Value.Has(lineMatchEntity))
                _destroyPool.Value.Add(lineMatchEntity).WaitTime = GameConfig.DESTROY_ANIMATION_TIME;
            _lineMatch.Pools.Inc3.Del(lineMatchEntity);
        }
    }

    private void LineSolver(PiecePosition startBlock, EcsPackedEntity[,] board,
        GameUtils.GetNextPosition nextPosition)
    {
        var nextPiece = new Stack<PiecePosition>();
        var discovered = new bool[board.GetLength(0), board.GetLength(1)];
        nextPiece.Push(startBlock);

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
            
            _destroyPool.Value.Add(destroyEntity).WaitTime = GameConfig.DESTROY_ANIMATION_TIME;
            nextPosition(position, ref nextPiece);
        }
    }
}