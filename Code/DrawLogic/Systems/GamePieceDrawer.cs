using System.Collections.Generic;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;

namespace MonoMatch3.Code.DrawLogic.Systems;

public class GamePieceDrawer : IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter _gamePieces;

    private EcsPool<GameLogic.Components.GamePieceType> _typePool;
    private EcsPool<GameLogic.Components.GamePiece> _piecePool;

    private SharedData _shared;
    private EcsWorld _world;

    private readonly Dictionary<GameLogic.Components.PieceType, Sprite> _tiles = new();
    private Point _tileSize;

    public void Init(IEcsSystems systems)
    {
        _shared = systems.GetShared<SharedData>();
        _world = systems.GetWorld();
        _gamePieces = _world.Filter<GameLogic.Components.GamePiece>().End();

        // pools
        _typePool = _world.GetPool<GameLogic.Components.GamePieceType>();
        _piecePool = _world.GetPool<GameLogic.Components.GamePiece>();

        // atlas related 
        _tileSize = DrawUtils.GetTileSize(_shared.TilesAtlas);

        for (int i = 0; i < 6; i++)
        {
            var rect = new Rectangle(_tileSize.X * i, DrawUtils.TILES_ROW, _tileSize.X, _tileSize.Y);
            _tiles[(GameLogic.Components.PieceType)i] = new Sprite(new TextureRegion2D(_shared.TilesAtlas, rect));
        }
    }

    public void Run(IEcsSystems systems)
    {
        _shared.SpriteBatch.Begin();

        foreach (var pieceEntity in _gamePieces)
        {
            ref var type = ref _typePool.Get(pieceEntity);
            ref var piece = ref _piecePool.Get(pieceEntity);
            _shared.SpriteBatch.Draw(_tiles[type.Type], piece.Transform);
        }

        _shared.SpriteBatch.End();
    }
}