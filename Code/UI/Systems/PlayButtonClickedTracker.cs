using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using MonoGame.Extended.Input;

namespace MonoMatch3.Code.UI.Systems;

public class PlayButtonClickedTracker : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<GameLogic.Components.GameStartState>> _gameStart = default;
    private readonly EcsFilterInject<Inc<Components.PlayButton>> _button = default;

    private readonly EcsPoolInject<GameLogic.Components.GameplayState> _gamePlayPool = default;

    private readonly EcsWorldInject _world = default;

    public void Run(IEcsSystems systems)
    {
        if (_gameStart.Value.GetEntitiesCount() < 1 || _button.Value.GetEntitiesCount() < 1)
            return;
        
        var mouseState = MouseExtended.GetState();
        if (!mouseState.WasButtonJustUp(MouseButton.Left))
            return;

        foreach (var entity in  _button.Value)
        {
            ref var button = ref _button.Pools.Inc1.Get(entity);
            if (button.Button.Bounds.Contains(mouseState.Position))
            {
                OnPlayButtonClick();
            }
        }
    }

    private void OnPlayButtonClick()
    {
        foreach (var entity in _gameStart.Value)
        {
            _gameStart.Pools.Inc1.Del(entity);
        }

        _gamePlayPool.Value.Add(_world.Value.NewEntity());
    }
}