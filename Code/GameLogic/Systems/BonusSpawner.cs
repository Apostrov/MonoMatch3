using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class BonusSpawner : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.BonusSpawn>> _bonusSpawn = default;
    private readonly EcsFilterInject<Inc<Components.GameBoard>> _gameBoard = default;

    private readonly EcsPoolInject<Components.GamePiece> _piecePool = default;
    private readonly EcsPoolInject<Components.Bonus> _bonusPool = default;
    private readonly EcsPoolInject<Components.Line> _linePool = default;
    private readonly EcsPoolInject<Components.Bomb> _bombPool = default;

    private readonly EcsSharedInject<SharedData> _shared = default;
    private readonly EcsWorldInject _world = default;

    public void Run(IEcsSystems systems)
    {
        if (_bonusSpawn.Value.GetEntitiesCount() < 1)
            return;

        foreach (var bonusSpawnEntity in _bonusSpawn.Value)
        {
            ref var bonusSpawn = ref _bonusSpawn.Pools.Inc1.Get(bonusSpawnEntity);
            bonusSpawn.WaitTime -= _shared.Value.GameTime.GetElapsedSeconds();
            if (bonusSpawn.WaitTime <= 0.0f)
            {
                if (bonusSpawn.Destroyed == GameConfig.LINE_BONUS_COUNT)
                {
                    var pieceEntity = _world.Value.NewEntity();
                    _linePool.Value.Add(pieceEntity).Type = GameUtils.GetRandomLine();
                    SpawnBonus(pieceEntity, bonusSpawn);
                }

                if (bonusSpawn.Destroyed >= GameConfig.BOMB_BONUS_COUNT)
                {
                    var pieceEntity = _world.Value.NewEntity();
                    _bombPool.Value.Add(pieceEntity);
                    SpawnBonus(pieceEntity, bonusSpawn);
                }

                _bonusSpawn.Pools.Inc1.Del(bonusSpawnEntity);
            }
        }
    }

    private void SpawnBonus(int pieceEntity, Components.BonusSpawn bonusSpawn)
    {
        foreach (var gameBoardEntity in _gameBoard.Value)
        {
            ref var gameBoard = ref _gameBoard.Pools.Inc1.Get(gameBoardEntity);
            int row = bonusSpawn.Position.Row, column = bonusSpawn.Position.Column;
            var tileSize = DrawLogic.DrawUtils.GetTileSize(_shared.Value.TilesAtlas);
            var position =
                DrawLogic.DrawUtils.GetTileScreenPosition(row, column, _shared.Value.GraphicsDevice, tileSize);

            ref var piece = ref _piecePool.Value.Add(pieceEntity);
            piece.BoardPosition.Column = column;
            piece.BoardPosition.Row = row;
            piece.Transform = new Transform2(position);
            piece.Radius = tileSize.X / 2f;

            _bonusPool.Value.Add(pieceEntity);

            var entityPacked = _world.Value.PackEntity(pieceEntity);
            gameBoard.Board[row, column] = entityPacked;
        }
    }
}