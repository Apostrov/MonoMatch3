using System;
using Leopotam.EcsLite;

namespace MonoMatch3.Code.GameLogic.Systems;

public class GameBoardInit : IEcsInitSystem
{
    private readonly int _boardSize;
    private readonly Random _random;

    private EcsWorld _world;

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

        var gameBoardPool = _world.GetPool<Components.GameBoard>();
        var positionPool = _world.GetPool<Components.GamePiecePosition>();
        var typePool = _world.GetPool<Components.GamePieceType>();

        ref var gameBoard = ref gameBoardPool.Add(_world.NewEntity());
        gameBoard.Board = new EcsPackedEntity[_boardSize, _boardSize];

        for (int row = 0; row < _boardSize; row++)
        {
            for (int column = 0; column < _boardSize; column++)
            {
                var pieceEntity = _world.NewEntity();

                ref var positionComponent = ref positionPool.Add(pieceEntity);
                positionComponent.Column = column;
                positionComponent.Row = row;

                ref var typeComponent = ref typePool.Add(pieceEntity);
                typeComponent.Type = GetRandomType();

                gameBoard.Board[row, column] = _world.PackEntity(pieceEntity);
            }
        }
    }

    private Components.PieceType GetRandomType()
    {
        return (Components.PieceType)_random.Next(0, (int)Components.PieceType.COUNT);
    }
}