using System.Collections.Generic;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class BombMatchProcessing : IEcsInitSystem, IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.Bomb, Components.BonusMatch>> _bombMatch =
        default;

    private readonly EcsFilterInject<Inc<Components.Bomb, Components.GamePiece, Components.BombExplosion>>
        _bombExplosion =
            default;

    private readonly EcsFilterInject<Inc<Components.GameBoard>> _gameBoard = default;

    private readonly EcsPoolInject<Components.BombExplosion> _explosionPool = default;
    private readonly EcsPoolInject<Components.DestroyPiece> _destroyPool = default;
    private readonly EcsPoolInject<Components.Bonus> _bonus = default;

    private readonly EcsWorldInject _world = default;
    private readonly EcsSharedInject<SharedData> _shared = default;

    private Algorithms.DFS _dfs;

    public void Init(IEcsSystems systems)
    {
        _dfs = new Algorithms.DistanceDFS(_bombExplosion.Pools.Inc2, _world.Value, GameConfig.BOMB_RADIUS);
    }

    public void Run(IEcsSystems systems)
    {
        foreach (var bombEntity in _bombMatch.Value)
        {
            if (!_explosionPool.Value.Has(bombEntity))
            {
                _explosionPool.Value.Add(bombEntity) = new Components.BombExplosion
                {
                    WaitTime = GameConfig.DESTROY_ANIMATION_TIME + GameConfig.BOMB_EXPLODE_TIMER,
                    Animation = _shared.Value.Tweener.TweenTo(
                        target: _bombExplosion.Pools.Inc2.Get(bombEntity).Transform,
                        expression: t => t.Scale,
                        toValue: Vector2.Zero,
                        duration: GameConfig.DESTROY_ANIMATION_TIME)
                };
            }

            _bombMatch.Pools.Inc2.Del(bombEntity);
        }

        foreach (var bombEntity in _bombExplosion.Value)
        {
            ref var explosion = ref _bombExplosion.Pools.Inc3.Get(bombEntity);
            explosion.WaitTime -= _shared.Value.GameTime.GetElapsedSeconds();
            if (explosion.WaitTime > 0.0f)
                continue;
            
            explosion.Animation.Cancel();

            foreach (var gameBoardEntity in _gameBoard.Value)
            {
                ref var gameBoard = ref _gameBoard.Pools.Inc1.Get(gameBoardEntity);
                var toDestroy = _dfs.Solve(_world.Value.PackEntity(bombEntity), gameBoard.Board, GetRadiusPosition());
                foreach (var entity in toDestroy)
                {
                    if (!_destroyPool.Value.Has(entity))
                        _destroyPool.Value.Add(entity).WaitTime = GameConfig.DESTROY_ANIMATION_TIME;
                    if (_bonus.Value.Has(entity) && !_bombMatch.Pools.Inc2.Has(entity))
                        _bombMatch.Pools.Inc2.Add(entity);
                }
            }

            if (!_destroyPool.Value.Has(bombEntity))
                _destroyPool.Value.Add(bombEntity);
            _bombExplosion.Pools.Inc3.Del(bombEntity);
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