using System;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using MonoGame.Extended.Input;

namespace MonoMatch3.Code.UI.Systems;

public class RestartButtonClickedTracker : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<GameLogic.Components.GameEndState>> _gameEnd = default;
    private readonly EcsFilterInject<Inc<Components.RestartButton>> _button = default;

    private readonly EcsPoolInject<GameLogic.Components.GameStartState> _gameStartPool = default;

    private readonly EcsWorldInject _world = default;
    private readonly Action OnRestart;

    public RestartButtonClickedTracker(Action onRestart)
    {
        OnRestart = onRestart;
    }

    public void Run(IEcsSystems systems)
    {
        if (_gameEnd.Value.GetEntitiesCount() < 1 || _button.Value.GetEntitiesCount() < 1)
            return;

        var mouseState = MouseExtended.GetState();
        if (!mouseState.WasButtonJustUp(MouseButton.Left))
            return;

        foreach (var entity in _button.Value)
        {
            ref var button = ref _button.Pools.Inc1.Get(entity);
            if (button.Button.Bounds.Contains(mouseState.Position))
            {
                OnRestart?.Invoke();
            }
        }
    }
}