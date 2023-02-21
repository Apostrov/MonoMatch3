using System;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class GameBoardInit : IEcsInitSystem
{
    private readonly EcsFilterInject<Inc<Components.GameBoard>> _existedBoards = default;

    private readonly EcsPoolInject<Components.GameBoard> _gameBoardPool = default;
    private readonly EcsPoolInject<Components.GamePiece> _piecePool = default;
    private readonly EcsPoolInject<Components.GamePieceType> _typePool = default;
    private readonly EcsPoolInject<Components.SolvePieceMatch> _solveMatch = default;

    private readonly EcsSharedInject<SharedData> _shared = default;
    private readonly EcsWorldInject _world = default;

    private Random _random;

    public void Init(IEcsSystems systems)
    {
        _random = new Random();

        // delete existed boards
        if (_existedBoards.Value.GetEntitiesCount() > 0)
        {
            foreach (var entity in _existedBoards.Value)
            {
                _world.Value.DelEntity(entity);
            }
        }

        // create board
        ref var gameBoard = ref _gameBoardPool.Value.Add(_world.Value.NewEntity());
        gameBoard.Board = new EcsPackedEntity[_shared.Value.BoardSize, _shared.Value.BoardSize];
        var tileSize = DrawLogic.DrawUtils.GetTileSize(_shared.Value.TilesAtlas);

        // fill board
        for (int row = 0; row < _shared.Value.BoardSize; row++)
        {
            for (int column = 0; column < _shared.Value.BoardSize; column++)
            {
                var pieceEntity = _world.Value.NewEntity();

                var position = DrawLogic.DrawUtils.GetTileScreenPosition(row, column, _shared.Value.GraphicsDevice,
                    tileSize, _shared.Value.BoardSize);
                ref var piece = ref _piecePool.Value.Add(pieceEntity);
                piece.BoardPosition.Column = column;
                piece.BoardPosition.Row = row;
                piece.Transform = new Transform2(position);
                piece.Radius = tileSize.X / 2f;

                ref var type = ref _typePool.Value.Add(pieceEntity);
                type.Type = GameUtils.GetRandomType();

                var entityPacked = _world.Value.PackEntity(pieceEntity);
                gameBoard.Board[row, column] = entityPacked;

                // solve match
                _solveMatch.Value.Add(_world.Value.NewEntity()).StartPiece = entityPacked;
            }
        }
    }
}