using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class GamePieceDestroyer : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.GamePiece, Components.DestroyPiece>> _destroyed = default;
    private readonly EcsFilterInject<Inc<Components.Score>> _score = default;

    private readonly EcsWorldInject _world = default;
    private readonly EcsSharedInject<SharedData> _shared = default;

    public void Run(IEcsSystems systems)
    {
        var score = 0;
        foreach (var entity in _destroyed.Value)
        {
            ref var destroy = ref _destroyed.Pools.Inc2.Get(entity);
            destroy.WaitTime -= _shared.Value.GameTime.GetElapsedSeconds();
            if (destroy.WaitTime <= 0.0f)
            {
                _world.Value.DelEntity(entity);
                score++;
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

        foreach (var entity in _score.Value)
        {
            _score.Pools.Inc1.Get(entity).Value += score;
        }
    }
}