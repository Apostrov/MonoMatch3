using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class GamePieceDestroyer : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.GamePiece, Components.DestroyPiece>> _destroyed = default;

    private readonly EcsWorldInject _world = default;
    private readonly EcsSharedInject<SharedData> _shared = default;

    public void Run(IEcsSystems systems)
    {
        foreach (var entity in _destroyed.Value)
        {
            ref var destroy = ref _destroyed.Pools.Inc2.Get(entity);
            destroy.WaitTime -= _shared.Value.GameTime.GetElapsedSeconds();
            if (destroy.WaitTime <= 0.0f)
            {
                _world.Value.DelEntity(entity);
            }
            else
            {
                destroy.Animation ??= _shared.Value.Tweener.TweenTo(
                    target: _destroyed.Pools.Inc1.Get(entity).Transform,
                    expression: t => t.Scale,
                    toValue: Vector2.Zero,
                    duration: destroy.WaitTime);
            }
        }
    }
}