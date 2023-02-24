using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class GameTimeDecreaser : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.GameplayState>> _gameplay = default;
    private readonly EcsFilterInject<Inc<Components.GameTimeRemaining>> _time = default;

    private readonly EcsPoolInject<Components.GameEndState> _gameEndPool = default;

    private readonly EcsWorldInject _world = default;
    private readonly EcsSharedInject<SharedData> _shared = default;

    public void Run(IEcsSystems systems)
    {
        if (_gameplay.Value.GetEntitiesCount() < 1)
            return;

        foreach (var timeEntity in _time.Value)
        {
            ref var time = ref _time.Pools.Inc1.Get(timeEntity);
            time.Value -= _shared.Value.GameTime.GetElapsedSeconds();
            if (time.Value <= 0.0f)
            {
                foreach (var gamePlayEntity in _gameplay.Value)
                {
                    _gameplay.Pools.Inc1.Del(gamePlayEntity);
                }

                _gameEndPool.Value.Add(_world.Value.NewEntity());
            }
        }
    }
}