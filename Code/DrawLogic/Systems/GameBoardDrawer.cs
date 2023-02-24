using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;

namespace MonoMatch3.Code.DrawLogic.Systems;

public class GameBoardDrawer : IEcsInitSystem, IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<GameLogic.Components.GameplayState>> _gameplay = default;
    private readonly EcsSharedInject<SharedData> _shared = default;

    private Sprite _backgroundTile;
    private Point _tileSize;

    public void Init(IEcsSystems systems)
    {
        // atlas related 
        _tileSize = DrawUtils.GetTileSize(_shared.Value.TilesAtlas);
        var backgroundRect = new Rectangle(0, _tileSize.Y * DrawUtils.BACKGROUND_ROW, _tileSize.X, _tileSize.Y);
        _backgroundTile = new Sprite(new TextureRegion2D(_shared.Value.TilesAtlas, backgroundRect));
    }

    public void Run(IEcsSystems systems)
    {
        if (_gameplay.Value.GetEntitiesCount() < 1)
            return;

        _shared.Value.SpriteBatch.Begin();

        for (int row = 0; row < GameConfig.BOARD_SIZE; row++)
        {
            for (int column = 0; column < GameConfig.BOARD_SIZE; column++)
            {
                var position = DrawUtils.GetTileScreenPosition(row, column, _shared.Value.GraphicsDevice, _tileSize);
                _shared.Value.SpriteBatch.Draw(_backgroundTile, position);
            }
        }

        _shared.Value.SpriteBatch.End();
    }
}