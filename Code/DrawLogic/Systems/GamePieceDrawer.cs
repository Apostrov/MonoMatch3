using System.Collections.Generic;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;

namespace MonoMatch3.Code.DrawLogic.Systems;

public class GamePieceDrawer : IEcsInitSystem, IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<GameLogic.Components.GamePiece, GameLogic.Components.GamePieceType>>
        _gamePieces = default;

    private readonly EcsSharedInject<SharedData> _shared = default;

    private readonly Dictionary<GameLogic.Components.PieceType, Sprite> _tiles = new();
    private Point _tileSize;

    public void Init(IEcsSystems systems)
    {
        // atlas related 
        _tileSize = DrawUtils.GetTileSize(_shared.Value.TilesAtlas);

        for (int i = 0; i < 6; i++)
        {
            var rect = new Rectangle(_tileSize.X * i, DrawUtils.TILES_ROW, _tileSize.X, _tileSize.Y);
            _tiles[(GameLogic.Components.PieceType)i] = new Sprite(new TextureRegion2D(_shared.Value.TilesAtlas, rect));
        }
    }

    public void Run(IEcsSystems systems)
    {
        _shared.Value.SpriteBatch.Begin();

        foreach (var pieceEntity in _gamePieces.Value)
        {
            ref var piece = ref _gamePieces.Pools.Inc1.Get(pieceEntity);
            ref var type = ref _gamePieces.Pools.Inc2.Get(pieceEntity);
            _shared.Value.SpriteBatch.Draw(_tiles[type.Type], piece.Transform);
        }

        _shared.Value.SpriteBatch.End();
    }
}