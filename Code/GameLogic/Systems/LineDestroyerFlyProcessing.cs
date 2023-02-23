using System;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class LineDestroyerFlyProcessing : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.LineDestroyer>> _destroyer = default;

    private readonly EcsPoolInject<Components.DestroyPiece> _destroyPool = default;
    private readonly EcsPoolInject<Components.Bonus> _bonus = default;
    private readonly EcsPoolInject<Components.BonusMatch> _bonusMatch = default;

    private readonly EcsWorldInject _world = default;
    private readonly EcsSharedInject<SharedData> _shared = default;

    // runtime data
    private Vector2 _direction;

    public void Run(IEcsSystems systems)
    {
        if (_destroyer.Value.GetEntitiesCount() < 1)
            return;

        foreach (var destroyerEntity in _destroyer.Value)
        {
            ref var destroyer = ref _destroyer.Pools.Inc1.Get(destroyerEntity);
            while (IsAlmostSameVector(destroyer.Transform.Position, destroyer.FlyPosition)
                   && destroyer.FlyPoints.Count > 0)
            {
                var flyPoint = destroyer.FlyPoints.Dequeue();
                destroyer.FlyPosition = flyPoint.position;
                if (flyPoint.packedEntity.Unpack(_world.Value, out var flyEntity) && !_destroyPool.Value.Has(flyEntity))
                {
                    if (_bonus.Value.Has(flyEntity) && !_bonusMatch.Value.Has(flyEntity))
                        _bonusMatch.Value.Add(flyEntity);
                    _destroyPool.Value.Add(flyEntity).WaitTime = GameConfig.DESTROY_ANIMATION_TIME;
                }
            }

            if (destroyer.FlyPoints.Count == 0 &&
                IsAlmostSameVector(destroyer.Transform.Position, destroyer.FlyPosition))
            {
                _destroyer.Pools.Inc1.Del(destroyerEntity);
                break;
            }

            _direction = destroyer.FlyPosition - destroyer.Transform.Position;
            _direction.Normalize();
            destroyer.Transform.Position += _direction * GameConfig.LINE_DESTROYER_FLY_SPEED *
                                            _shared.Value.GameTime.GetElapsedSeconds();
        }
    }

    private bool IsAlmostSameVector(Vector2 vector1, Vector2 vector2)
    {
        return Math.Abs(vector1.X - vector2.X) < 0.15f && Math.Abs(vector1.Y - vector2.Y) < 0.15f;
    }
}