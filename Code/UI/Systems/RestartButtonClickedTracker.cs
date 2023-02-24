using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using MonoGame.Extended.Input;

namespace MonoMatch3.Code.UI.Systems;

public class RestartButtonClickedTracker: IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<GameLogic.Components.GameEndState>> _gameEnd = default;
    private readonly EcsFilterInject<Inc<Components.RestartButton>> _button = default;

    private readonly EcsPoolInject<GameLogic.Components.GameStartState> _gameStartPool = default;

    private readonly EcsWorldInject _world = default;

    public void Run(IEcsSystems systems)
    {
        if (_gameEnd.Value.GetEntitiesCount() < 1 || _button.Value.GetEntitiesCount() < 1)
            return;
        
        var mouseState = MouseExtended.GetState();
        if (!mouseState.WasButtonJustUp(MouseButton.Left))
            return;

        foreach (var entity in  _button.Value)
        {
            ref var button = ref _button.Pools.Inc1.Get(entity);
            if (button.Button.Bounds.Contains(mouseState.Position))
            {
                OnRestartButtonClick();
            }
        }
    }

    private void OnRestartButtonClick()
    {
        foreach (var entity in _gameEnd.Value)
        {
            _gameEnd.Pools.Inc1.Del(entity);
        }

        _gameStartPool.Value.Add(_world.Value.NewEntity());
    }
}