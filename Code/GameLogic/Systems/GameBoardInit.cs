using System;
using Leopotam.EcsLite;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class GameBoardInit : IEcsInitSystem
{
    private EcsPool<Components.GameBoard> _gameBoardPool;
    private EcsPool<Components.GamePiece> _piecePool;
    private EcsPool<Components.GamePieceType> _typePool;
    private EcsPool<Components.SolveMatch> _solveMatch;

    private EcsWorld _world;
    private Random _random;
    private SharedData _shared;

    public void Init(IEcsSystems systems)
    {
        _random = new Random();
        _world = systems.GetWorld();
        _shared = systems.GetShared<SharedData>();
        
        // delete existed boards
        var existedBoards = _world.Filter<Components.GameBoard>().End();
        if (existedBoards.GetEntitiesCount() > 0)
        {
            foreach (var entity in existedBoards)
            {
                _world.DelEntity(entity);
            }
        }
        
        // pools
        _gameBoardPool = _world.GetPool<Components.GameBoard>();
        _piecePool = _world.GetPool<Components.GamePiece>();
        _typePool = _world.GetPool<Components.GamePieceType>();
        _solveMatch = _world.GetPool<Components.SolveMatch>();
        
        // create board
        ref var gameBoard = ref _gameBoardPool.Add(_world.NewEntity());
        gameBoard.Board = new EcsPackedEntity[_shared.BoardSize, _shared.BoardSize];
        var tileSize = DrawLogic.DrawUtils.GetTileSize(_shared.TilesAtlas);
        
        // fill board
        for (int row = 0; row < _shared.BoardSize; row++)
        {
            for (int column = 0; column < _shared.BoardSize; column++)
            {
                var pieceEntity = _world.NewEntity();

                var position = DrawLogic.DrawUtils.GetTileScreenPosition(row, column, _shared.GraphicsDevice, tileSize,
                    _shared.BoardSize);
                ref var piece = ref _piecePool.Add(pieceEntity);
                piece.Column = column;
                piece.Row = row;
                piece.Transform = new Transform2(position);
                piece.Radius = tileSize.X / 2f;

                ref var type = ref _typePool.Add(pieceEntity);
                type.Type = GetRandomType();

                gameBoard.Board[row, column] = _world.PackEntity(pieceEntity);
            }
        }
        
        // solve match
        _solveMatch.Add(_world.NewEntity());
    }

    private Components.PieceType GetRandomType()
    {
        return (Components.PieceType)_random.Next(0, (int)Components.PieceType.COUNT);
    }
}