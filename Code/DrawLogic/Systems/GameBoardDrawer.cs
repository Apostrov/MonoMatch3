using System.Collections.Generic;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;

namespace MonoMatch3.Code.DrawLogic.Systems;

public class GameBoardDrawer : IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter _gameBoard;

    private SharedData _shared;
    private EcsWorld _world;
    private Rectangle _backgroundRect;

    // to get ATLAS_SIZE, BACKGROUND_INDEX, tilesRect look at atlas
    private const int ATLAS_SIZE = 7;
    private const int BACKGROUND_INDEX = 5;
    private readonly Dictionary<GameLogic.Components.PieceType, Rectangle> _tilesRect = new();

    public void Init(IEcsSystems systems)
    {
        _shared = systems.GetShared<SharedData>();
        _world = systems.GetWorld();
        _gameBoard = _world.Filter<GameLogic.Components.GameBoard>().End();

        int width = _shared.TilesAtlas.Width / ATLAS_SIZE;
        int height = _shared.TilesAtlas.Height / ATLAS_SIZE;
        _backgroundRect = new Rectangle(0, height * BACKGROUND_INDEX, width, height);

        for (int i = 0; i < 6; i++)
        {
            _tilesRect[(GameLogic.Components.PieceType)i] = new Rectangle(width * i, 0, width, height);
        }
    }

    public void Run(IEcsSystems systems)
    {
        _shared.SpriteBatch.Begin();

        // pools
        var gameBoardPool = _world.GetPool<GameLogic.Components.GameBoard>();
        var typePool = _world.GetPool<GameLogic.Components.GamePieceType>();
        var positionPool = _world.GetPool<GameLogic.Components.GamePiecePosition>();

        foreach (var boardEntity in _gameBoard)
        {
            ref var gameBoard = ref gameBoardPool.Get(boardEntity);
            var boardSize = gameBoard.Board.GetLength(0);

            // Calculate the position of the top-left corner of the grid
            int tileSize =
                MathHelper.Min(_shared.GraphicsDevice.Viewport.Height, _shared.GraphicsDevice.Viewport.Width) /
                boardSize;
            int centerX = _shared.GraphicsDevice.Viewport.Width / 2;
            int centerY = _shared.GraphicsDevice.Viewport.Height / 2;
            int startX = centerX - tileSize * boardSize / 2;
            int startY = centerY - tileSize * boardSize / 2;


            for (int row = 0; row < boardSize; row++)
            {
                for (int column = 0; column < boardSize; column++)
                {
                    var destinationRect =
                        new Rectangle(startX + column * tileSize, startY + row * tileSize, tileSize, tileSize);

                    // background
                    _shared.SpriteBatch.Draw(_shared.TilesAtlas, destinationRect, _backgroundRect, Color.White);

                    // piece
                    var pieceEntityPacked = gameBoard.Board[row, column];
                    if (pieceEntityPacked.Unpack(_world, out var pieceEntity))
                    {
                        ref var type = ref typePool.Get(pieceEntity);
                        positionPool.Get(pieceEntity).DrawnPosition = destinationRect;
                        _shared.SpriteBatch.Draw(_shared.TilesAtlas, destinationRect, _tilesRect[type.Type],
                            Color.White);
                    }
                }
            }
        }

        _shared.SpriteBatch.End();
    }
}