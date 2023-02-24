using System.Collections.Generic;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace MonoMatch3.Code.GameLogic.Systems;

public class BonusMatchProcessing : IEcsInitSystem, IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.Bomb, Components.GamePiece, Components.BonusMatch>> _bombMatch =
        default;

    private readonly EcsFilterInject<Inc<Components.GameBoard>> _gameBoard = default;

    private readonly EcsPoolInject<Components.DestroyPiece> _destroyPool = default;
    private readonly EcsPoolInject<Components.Bonus> _bonus = default;

    private readonly EcsWorldInject _world = default;

    private Algorithms.DFS _dfs;

    public void Init(IEcsSystems systems)
    {
        _dfs = new Algorithms.DistanceDFS(_bombMatch.Pools.Inc2, _world.Value, GameConfig.BOMB_RADIUS);
    }

    public void Run(IEcsSystems systems)
    {
        foreach (var bombEntity in _bombMatch.Value)
        {
            foreach (var gameBoardEntity in _gameBoard.Value)
            {
                ref var gameBoard = ref _gameBoard.Pools.Inc1.Get(gameBoardEntity);
                var toDestroy = _dfs.Solve(_world.Value.PackEntity(bombEntity), gameBoard.Board, GetRadiusPosition());
                foreach (var entity in toDestroy)
                {
                    if (!_destroyPool.Value.Has(entity))
                        _destroyPool.Value.Add(entity).WaitTime = GameConfig.DESTROY_ANIMATION_TIME;
                    if (_bonus.Value.Has(entity) && !_bombMatch.Pools.Inc3.Has(entity))
                        _bombMatch.Pools.Inc3.Add(entity);
                }
            }

            if (!_destroyPool.Value.Has(bombEntity))
                _destroyPool.Value.Add(bombEntity).WaitTime = GameConfig.DESTROY_ANIMATION_TIME;
            _bombMatch.Pools.Inc3.Del(bombEntity);
        }
    }

    private Algorithms.DFS.GetNextPosition GetRadiusPosition()
    {
        return (PiecePosition position, ref Stack<PiecePosition> add) =>
        {
            add.Push(new PiecePosition(position.Row + 1, position.Column));
            add.Push(new PiecePosition(position.Row - 1, position.Column));
            add.Push(new PiecePosition(position.Row, position.Column + 1));
            add.Push(new PiecePosition(position.Row, position.Column - 1));
            add.Push(new PiecePosition(position.Row + 1, position.Column + 1));
            add.Push(new PiecePosition(position.Row + 1, position.Column - 1));
            add.Push(new PiecePosition(position.Row - 1, position.Column + 1));
            add.Push(new PiecePosition(position.Row - 1, position.Column - 1));
        };
    }
}