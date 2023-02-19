using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;

namespace MonoMatch3.Code.DrawLogic.Systems;

public class GameBoardDrawer : IEcsInitSystem, IEcsRunSystem
{
    private SharedData _shared;

    private Sprite _backgroundTile;
    private Point _tileSize;

    public void Init(IEcsSystems systems)
    {
        _shared = systems.GetShared<SharedData>();

        // atlas related 
        _tileSize = DrawUtils.GetTileSize(_shared.TilesAtlas);
        var backgroundRect = new Rectangle(0, _tileSize.Y * DrawUtils.BACKGROUND_ROW, _tileSize.X, _tileSize.Y);
        _backgroundTile = new Sprite(new TextureRegion2D(_shared.TilesAtlas, backgroundRect));
    }

    public void Run(IEcsSystems systems)
    {
        _shared.SpriteBatch.Begin();

        for (int row = 0; row < _shared.BoardSize; row++)
        {
            for (int column = 0; column < _shared.BoardSize; column++)
            {
                var position = DrawUtils.GetTileScreenPosition(row, column, _shared.GraphicsDevice, _tileSize,
                    _shared.BoardSize);
                _shared.SpriteBatch.Draw(_backgroundTile, position);
            }
        }

        _shared.SpriteBatch.End();
    }
}