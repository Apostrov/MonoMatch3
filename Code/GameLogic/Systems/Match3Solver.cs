using System.Collections.Generic;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class Match3Solver : IEcsInitSystem, IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.GameBoard>> _gameBoard = default;
    private readonly EcsFilterInject<Inc<Components.SolvePieceMatch>> _solveMatch = default;

    private readonly EcsPoolInject<Components.GamePiece> _piecePool = default;
    private readonly EcsPoolInject<Components.GamePieceType> _pieceTypePool = default;
    private readonly EcsPoolInject<Components.DestroyPiece> _destroyPool = default;
    private readonly EcsPoolInject<Components.RearrangeBoard> _rearrangePool = default;
    private readonly EcsPoolInject<Components.BonusSpawn> _bonusSpawnPool = default;
    private readonly EcsPoolInject<Components.Bonus> _bonusPool = default;
    private readonly EcsPoolInject<Components.BonusMatch> _bonusMatchPool = default;

    private readonly EcsSharedInject<SharedData> _shared = default;
    private readonly EcsWorldInject _world = default;

    private Algorithms.DFS _dfs;

    public void Init(IEcsSystems systems)
    {
        _dfs = new Algorithms.ColorDFS(_piecePool.Value, _world.Value, _pieceTypePool.Value);
    }

    public void Run(IEcsSystems systems)
    {
        if (_solveMatch.Value.GetEntitiesCount() < 1)
            return;

        foreach (var boardEntity in _gameBoard.Value)
        {
            ref var board = ref _gameBoard.Pools.Inc1.Get(boardEntity);
            var bonusPlayed = false;
            foreach (var solveEntity in _solveMatch.Value)
            {
                ref var solveMatch = ref _solveMatch.Pools.Inc1.Get(solveEntity);
                solveMatch.WaitTime -= _shared.Value.GameTime.GetElapsedSeconds();
                if (solveMatch.WaitTime > 0.0f)
                    continue;

                if (solveMatch.IsClicked)
                    bonusPlayed |= IsBonusMatch(solveMatch.StartPiece);

                var rowToDestroy = _dfs.Solve(solveMatch.StartPiece, board.Board, RowMover());
                var columnToDestroy = _dfs.Solve(solveMatch.StartPiece, board.Board, ColumnMover());
                int destroyed = DestroyLine(rowToDestroy);
                destroyed += DestroyLine(columnToDestroy);
                MatchCompleted(destroyed, bonusPlayed, ref solveMatch);

                _solveMatch.Pools.Inc1.Del(solveEntity);
            }
        }
    }

    private bool IsBonusMatch(EcsPackedEntity startPiece)
    {
        if (!startPiece.Unpack(_world.Value, out var startPieceEntity) || !_bonusPool.Value.Has(startPieceEntity))
            return false;

        if (!_bonusMatchPool.Value.Has(startPieceEntity))
            _bonusMatchPool.Value.Add(startPieceEntity);
        return true;
    }

    private void MatchCompleted(int destroyedNumber, bool bonusPlayed, ref Components.SolvePieceMatch solveMatch)
    {
        if (destroyedNumber > 0 || bonusPlayed)
        {
            if (solveMatch.StartPiece.Unpack(_world.Value, out var entity))
            {
                _bonusSpawnPool.Value.Add(_world.Value.NewEntity()) = new Components.BonusSpawn
                {
                    Destroyed = destroyedNumber,
                    Position = _piecePool.Value.Get(entity).BoardPosition,
                    WaitTime = GameConfig.DESTROY_ANIMATION_TIME
                };
            }

            _rearrangePool.Value.Add(_world.Value.NewEntity()).WaitTime = GameConfig.DESTROY_ANIMATION_TIME;
        }
        else
        {
            solveMatch.OnDontMatchCallback?.Invoke();
        }
    }


    private int DestroyLine(List<int> entities)
    {
        if (entities.Count < GameConfig.MATCH_COUNT)
            return 0;

        int destroyed = 0;
        foreach (var entity in entities)
        {
            if (_destroyPool.Value.Has(entity))
                continue;
            _destroyPool.Value.Add(entity).WaitTime = GameConfig.DESTROY_ANIMATION_TIME;
            destroyed++;
        }

        return destroyed;
    }

    private Algorithms.DFS.GetNextPosition RowMover()
    {
        return (PiecePosition piecePosition, ref Stack<PiecePosition> add) =>
        {
            add.Push(new PiecePosition(piecePosition.Row + 1, piecePosition.Column));
            add.Push(new PiecePosition(piecePosition.Row - 1, piecePosition.Column));
        };
    }

    private Algorithms.DFS.GetNextPosition ColumnMover()
    {
        return (PiecePosition piecePosition, ref Stack<PiecePosition> add) =>
        {
            add.Push(new PiecePosition(piecePosition.Row, piecePosition.Column + 1));
            add.Push(new PiecePosition(piecePosition.Row, piecePosition.Column - 1));
        };
    }
}