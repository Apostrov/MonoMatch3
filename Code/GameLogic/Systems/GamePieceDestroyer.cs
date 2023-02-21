using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class GamePieceDestroyer : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.DestroyPiece>> _destroyed = default;

    private readonly EcsWorldInject _world = default;
    private readonly EcsSharedInject<SharedData> _shared = default;

    public void Run(IEcsSystems systems)
    {
        foreach (var entity in _destroyed.Value)
        {
            ref var piece = ref _destroyed.Pools.Inc1.Get(entity);
            piece.WaitTime -= _shared.Value.GameTime.GetElapsedSeconds();
            if (piece.WaitTime <= 0.0f)
            {
                _world.Value.DelEntity(entity);
            }
        }
    }
}