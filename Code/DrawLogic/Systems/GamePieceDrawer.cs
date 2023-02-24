using System.Collections.Generic;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;

namespace MonoMatch3.Code.DrawLogic.Systems;

public class GamePieceDrawer : IEcsInitSystem, IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<GameLogic.Components.GameplayState>> _gameplay = default;
    private readonly EcsFilterInject<Inc<GameLogic.Components.GamePiece>> _gamePieces = default;

    private readonly EcsPoolInject<GameLogic.Components.GamePieceType> _typePool = default;
    private readonly EcsPoolInject<GameLogic.Components.Line> _linePool = default;
    private readonly EcsPoolInject<GameLogic.Components.Bomb> _bombPool = default;

    private readonly EcsSharedInject<SharedData> _shared = default;

    private readonly Dictionary<GameLogic.PieceType, Sprite> _tiles = new();
    private readonly Dictionary<GameLogic.LineType, Sprite> _line = new();
    private Sprite _bomb;
    private Point _tileSize;

    public void Init(IEcsSystems systems)
    {
        // atlas related 
        _tileSize = DrawUtils.GetTileSize(_shared.Value.TilesAtlas);

        for (int i = 0; i < (int)GameLogic.PieceType.COUNT; i++)
        {
            var rect = new Rectangle(_tileSize.X * i, _tileSize.Y * DrawUtils.TILES_ROW, _tileSize.X, _tileSize.Y);
            _tiles[(GameLogic.PieceType)i] = new Sprite(new TextureRegion2D(_shared.Value.TilesAtlas, rect));
        }

        for (int i = 0; i < (int)GameLogic.LineType.COUNT; i++)
        {
            var rect = new Rectangle(_tileSize.X * i, _tileSize.Y * DrawUtils.BONUSES_ROW, _tileSize.X, _tileSize.Y);
            _line[(GameLogic.LineType)i] = new Sprite(new TextureRegion2D(_shared.Value.TilesAtlas, rect));
        }

        var bombRect = new Rectangle(_tileSize.X * DrawUtils.BOMB_COLUMN, _tileSize.Y * DrawUtils.BONUSES_ROW,
            _tileSize.X, _tileSize.Y);
        _bomb = new Sprite(new TextureRegion2D(_shared.Value.TilesAtlas, bombRect));
    }

    public void Run(IEcsSystems systems)
    {
        if(_gameplay.Value.GetEntitiesCount() < 1)
            return;
        
        _shared.Value.SpriteBatch.Begin();

        foreach (var pieceEntity in _gamePieces.Value)
        {
            ref var piece = ref _gamePieces.Pools.Inc1.Get(pieceEntity);
            if (_typePool.Value.Has(pieceEntity))
            {
                ref var type = ref _typePool.Value.Get(pieceEntity);
                _shared.Value.SpriteBatch.Draw(_tiles[type.Type], piece.Transform);
            }

            if (_linePool.Value.Has(pieceEntity))
            {
                ref var type = ref _linePool.Value.Get(pieceEntity);
                _shared.Value.SpriteBatch.Draw(_line[type.Type], piece.Transform);
            }

            if (_bombPool.Value.Has(pieceEntity))
            {
                _shared.Value.SpriteBatch.Draw(_bomb, piece.Transform);
            }
        }

        _shared.Value.SpriteBatch.End();
    }
}