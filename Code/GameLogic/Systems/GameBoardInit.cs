using System;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class GameBoardInit : IEcsInitSystem
{
    private EcsPool<Components.GameBoard> _gameBoardPool;
    private EcsPool<Components.GamePiece> _piecePool;
    private EcsPool<Components.GamePieceType> _typePool;

    private EcsWorld _world;

    private readonly int _boardSize;
    private readonly Random _random;


    public GameBoardInit(int boardSize)
    {
        _boardSize = boardSize;
        _random = new Random();
    }

    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();
        var existedBoards = _world.Filter<Components.GameBoard>().End();
        if (existedBoards.GetEntitiesCount() > 0)
        {
            foreach (var entity in existedBoards)
            {
                _world.DelEntity(entity);
            }
        }

        _gameBoardPool = _world.GetPool<Components.GameBoard>();
        _piecePool = _world.GetPool<Components.GamePiece>();
        _typePool = _world.GetPool<Components.GamePieceType>();

        ref var gameBoard = ref _gameBoardPool.Add(_world.NewEntity());
        gameBoard.Board = new EcsPackedEntity[_boardSize, _boardSize];

        for (int row = 0; row < _boardSize; row++)
        {
            for (int column = 0; column < _boardSize; column++)
            {
                var pieceEntity = _world.NewEntity();

                ref var piece = ref _piecePool.Add(pieceEntity);
                piece.Column = column;
                piece.Row = row;
                piece.Transform = new Transform2();

                ref var type = ref _typePool.Add(pieceEntity);
                type.Type = GetRandomType();

                gameBoard.Board[row, column] = _world.PackEntity(pieceEntity);
            }
        }
    }

    private Components.PieceType GetRandomType()
    {
        return (Components.PieceType)_random.Next(0, (int)Components.PieceType.COUNT);
    }
}