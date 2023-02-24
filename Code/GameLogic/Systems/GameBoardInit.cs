using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class GameBoardInit : IEcsInitSystem
{
    private readonly EcsFilterInject<Inc<Components.GameBoard>> _existedBoards = default;
    private readonly EcsFilterInject<Inc<Components.Score>> _score = default;
    private readonly EcsFilterInject<Inc<Components.GameTimeRemaining>> _time = default;

    private readonly EcsPoolInject<Components.GameBoard> _gameBoardPool = default;
    private readonly EcsPoolInject<Components.GamePiece> _piecePool = default;
    private readonly EcsPoolInject<Components.GamePieceType> _typePool = default;
    private readonly EcsPoolInject<Components.SolvePieceMatch> _solveMatchPool = default;

    private readonly EcsSharedInject<SharedData> _shared = default;
    private readonly EcsWorldInject _world = default;

    public void Init(IEcsSystems systems)
    {
        // delete existed boards
        foreach (var entity in _existedBoards.Value)
        {
            _world.Value.DelEntity(entity);
        }

        // create board
        ref var gameBoard = ref _gameBoardPool.Value.Add(_world.Value.NewEntity());
        gameBoard.Board = new EcsPackedEntity[GameConfig.BOARD_SIZE, GameConfig.BOARD_SIZE];
        var tileSize = DrawLogic.DrawUtils.GetTileSize(_shared.Value.TilesAtlas);

        // fill board
        for (int row = 0; row < GameConfig.BOARD_SIZE; row++)
        {
            for (int column = 0; column < GameConfig.BOARD_SIZE; column++)
            {
                var pieceEntity = _world.Value.NewEntity();

                var position = DrawLogic.DrawUtils.GetTileScreenPosition(row, column, _shared.Value.GraphicsDevice,
                    tileSize);
                ref var piece = ref _piecePool.Value.Add(pieceEntity);
                piece.BoardPosition.Column = column;
                piece.BoardPosition.Row = row;
                piece.Transform = new Transform2(position);
                piece.Radius = tileSize.X / 2f;

                ref var type = ref _typePool.Value.Add(pieceEntity);
                type.Type = GameUtils.GetRandomPiece();

                var entityPacked = _world.Value.PackEntity(pieceEntity);
                gameBoard.Board[row, column] = entityPacked;

                // solve match
                _solveMatchPool.Value.Add(_world.Value.NewEntity()).StartPiece = entityPacked;
            }
        }

        // delete existed game scores
        foreach (var entity in _score.Value)
        {
            _score.Pools.Inc1.Del(entity);
        }

        foreach (var entity in _time.Value)
        {
            _time.Pools.Inc1.Del(entity);
        }

        // create game scores
        var gameScoresEntity = _world.Value.NewEntity();
        _score.Pools.Inc1.Add(gameScoresEntity).Value = 0;
        _time.Pools.Inc1.Add(gameScoresEntity).Value = GameConfig.GAME_TIME;
    }
}