using System;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;

namespace MonoMatch3.Code.UI.Systems;

public class GameplayUIDrawer : IEcsInitSystem, IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<GameLogic.Components.GameplayState>> _gameplay = default;
    private readonly EcsFilterInject<Inc<GameLogic.Components.Score>> _gameScore = default;
    private readonly EcsFilterInject<Inc<GameLogic.Components.GameTimeRemaining>> _gameTime = default;

    private readonly EcsSharedInject<SharedData> _shared = default;

    private Model.Label _score;
    private Model.Label _time;

    public void Init(IEcsSystems systems)
    {
        // score
        var scoreText = "Score: ";
        var scorePosition = new Vector2(10f, 10f);
        _score = new Model.Label(scoreText, scorePosition);

        // time
        var timeText = "Time: ";
        var timePosition = new Vector2(10f, 100f);
        _time = new Model.Label(timeText, timePosition);
    }

    public void Run(IEcsSystems systems)
    {
        if (_gameplay.Value.GetEntitiesCount() < 1)
            return;

        _shared.Value.SpriteBatch.Begin();

        var score = 0;
        var timeLeft = 60f;

        foreach (var entity in _gameScore.Value)
        {
            score = _gameScore.Pools.Inc1.Get(entity).Value;
        }

        foreach (var entity in _gameTime.Value)
        {
            timeLeft = _gameTime.Pools.Inc1.Get(entity).Value;
        }

        _shared.Value.SpriteBatch.DrawString(_shared.Value.Font, _score.Text + score, _score.Position,
            Color.Black);
        _shared.Value.SpriteBatch.DrawString(_shared.Value.Font, _time.Text + MathF.Round(timeLeft), _time.Position,
            Color.Black);

        _shared.Value.SpriteBatch.End();
    }
}