using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MonoMatch3.Code.UI.Systems;

public class MenuUIDrawer : IEcsInitSystem, IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<GameLogic.Components.GameStartState>> _gameStart = default;

    private readonly EcsPoolInject<Components.PlayButton> _playButtonPool = default;

    private readonly EcsWorldInject _world = default;
    private readonly EcsSharedInject<SharedData> _shared = default;

    private Model.Button _playButton;
    private Model.Label _gameNameLabel;

    public void Init(IEcsSystems systems)
    {
        var graphicsDevice = _shared.Value.GraphicsDevice;

        // Button
        var buttonText = "Play";
        var buttonSize = new Vector2(250f, 75f);
        var buttonPosition = new Vector2(graphicsDevice.Viewport.Width / 2f - buttonSize.X / 2f,
            graphicsDevice.Viewport.Height / 2f - buttonSize.Y / 2f + 100f);
        var buttonRect = new Rectangle(buttonPosition.ToPoint(), buttonSize.ToPoint());
        var textPosition = new Vector2(buttonRect.X + buttonRect.Width / 2f, buttonRect.Y + buttonRect.Height / 2f);
        _playButton = new Model.Button(buttonText, buttonRect, textPosition);
        _playButtonPool.Value.Add(_world.Value.NewEntity()).Button = _playButton;

        // Label
        var labelText = "MonoMatch3";
        var textSize = _shared.Value.Font.MeasureString(labelText);
        var labelPosition =
            new Vector2(graphicsDevice.Viewport.Width / 2f - textSize.X / 2f,
                graphicsDevice.Viewport.Height / 2f - textSize.Y / 2f - 100f);
        _gameNameLabel = new Model.Label(labelText, labelPosition);
    }

    public void Run(IEcsSystems systems)
    {
        if (_gameStart.Value.GetEntitiesCount() < 1)
            return;

        _shared.Value.SpriteBatch.Begin();

        _shared.Value.SpriteBatch.DrawString(_shared.Value.Font, _gameNameLabel.Text, _gameNameLabel.Position,
            Color.Black);
        _shared.Value.SpriteBatch.DrawRectangle(_playButton.Bounds, Color.Black);
        _shared.Value.SpriteBatch.DrawString(_shared.Value.Font, _playButton.Text, _playButton.TextPosition,
            Color.Black, 0f, _shared.Value.Font.MeasureString(_playButton.Text) / 2f, 1f, SpriteEffects.None, 0f);

        _shared.Value.SpriteBatch.End();
    }
}