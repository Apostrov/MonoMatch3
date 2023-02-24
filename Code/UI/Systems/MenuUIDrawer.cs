using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoMatch3.Code.UI.Systems;

public class MenuUIDrawer : IEcsInitSystem, IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<GameLogic.Components.GameStartState>> _gameStart = default;
    private readonly EcsSharedInject<SharedData> _shared = default;

    private Vector2 _center;
    private Size2 _buttonSize;

    public void Init(IEcsSystems systems)
    {
        int centerX = _shared.Value.GraphicsDevice.Viewport.Width / 2;
        int centerY = _shared.Value.GraphicsDevice.Viewport.Height / 2;
        _center = new Vector2(centerX, centerY);
    }

    public void Run(IEcsSystems systems)
    {
        if (_gameStart.Value.GetEntitiesCount() < 1)
            return;

        _shared.Value.SpriteBatch.Begin();

        _shared.Value.SpriteBatch.DrawRectangle(_center + new Vector2(-250f, 0f), new Size2(500f, 100f), Color.Black);
        _shared.Value.SpriteBatch.DrawString(_shared.Value.Font, $"MonoMatch3", _center + new Vector2(0f, -100f),
            Color.Black);

        _shared.Value.SpriteBatch.End();
    }
}