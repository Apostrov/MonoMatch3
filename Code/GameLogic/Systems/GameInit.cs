using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace MonoMatch3.Code.GameLogic.Systems;

public class GameInit : IEcsInitSystem
{
    private readonly EcsPoolInject<Components.GameStartState> _uiState = default;
    private readonly EcsWorldInject _world = default;

    public void Init(IEcsSystems systems)
    {
        _uiState.Value.Add(_world.Value.NewEntity());
    }
}