using System.Collections.Generic;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;

namespace MonoMatch3.Code.DrawLogic.Systems;

public class GameBoardDrawer : IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter _gameBoard;

    private EcsPool<GameLogic.Components.GameBoard> _gameBoardPool;
    private EcsPool<GameLogic.Components.GamePieceType> _typePool;
    private EcsPool<GameLogic.Components.GamePiece> _piecePool;
    private EcsPool<GameLogic.Components.Clicked> _clickedPool;

    private SharedData _shared;
    private EcsWorld _world;

    // to get ATLAS_SIZE, BACKGROUND_INDEX, tilesRect look at atlas
    private const int ATLAS_SIZE = 7;
    private const int BACKGROUND_INDEX = 5;
    private Sprite _backgroundTile;
    private readonly Dictionary<GameLogic.Components.PieceType, Sprite> _tiles = new();
    private Point _tileSize;

    public void Init(IEcsSystems systems)
    {
        _shared = systems.GetShared<SharedData>();
        _world = systems.GetWorld();
        _gameBoard = _world.Filter<GameLogic.Components.GameBoard>().End();

        // pools
        _gameBoardPool = _world.GetPool<GameLogic.Components.GameBoard>();
        _typePool = _world.GetPool<GameLogic.Components.GamePieceType>();
        _piecePool = _world.GetPool<GameLogic.Components.GamePiece>();
        _clickedPool = _world.GetPool<GameLogic.Components.Clicked>();

        // atlas related 
        _tileSize.X = _shared.TilesAtlas.Width / ATLAS_SIZE;
        _tileSize.Y = _shared.TilesAtlas.Height / ATLAS_SIZE;
        var backgroundRect = new Rectangle(0, _tileSize.Y * BACKGROUND_INDEX, _tileSize.X, _tileSize.Y);
        _backgroundTile = new Sprite(new TextureRegion2D(_shared.TilesAtlas, backgroundRect));

        for (int i = 0; i < 6; i++)
        {
            var rect = new Rectangle(_tileSize.X * i, 0, _tileSize.X, _tileSize.Y);
            _tiles[(GameLogic.Components.PieceType)i] = new Sprite(new TextureRegion2D(_shared.TilesAtlas, rect));
        }
    }

    public void Run(IEcsSystems systems)
    {
        _shared.SpriteBatch.Begin();

        foreach (var boardEntity in _gameBoard)
        {
            ref var gameBoard = ref _gameBoardPool.Get(boardEntity);
            var boardSize = gameBoard.Board.GetLength(0);

            // Calculate the position of the top-left corner of the grid
            int centerX = _shared.GraphicsDevice.Viewport.Width / 2 + _tileSize.X / 2;
            int centerY = _shared.GraphicsDevice.Viewport.Height / 2 + _tileSize.Y / 2;
            int startX = centerX - _tileSize.X * boardSize / 2;
            int startY = centerY - _tileSize.Y * boardSize / 2;

            for (int row = 0; row < boardSize; row++)
            {
                for (int column = 0; column < boardSize; column++)
                {
                    var position = new Vector2(startX + column * _tileSize.X, startY + row * _tileSize.Y);

                    // background
                    _shared.SpriteBatch.Draw(_backgroundTile, position);

                    // piece
                    var pieceEntityPacked = gameBoard.Board[row, column];
                    if (pieceEntityPacked.Unpack(_world, out var pieceEntity))
                    {
                        
                        ref var type = ref _typePool.Get(pieceEntity);
                        ref var piece = ref _piecePool.Get(pieceEntity);
                        if (_clickedPool.Has(pieceEntity))
                            piece.Transform.Scale = new Vector2(0.5f, 0.5f);
                        piece.Transform.Position = position;
                        piece.Radius = _tileSize.X / 2f;
                        _shared.SpriteBatch.Draw(_tiles[type.Type], piece.Transform);
                    }
                }
            }
        }

        _shared.SpriteBatch.End();
    }
}