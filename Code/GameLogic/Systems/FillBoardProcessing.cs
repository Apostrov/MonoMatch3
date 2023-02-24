using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class FillBoardProcessing : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.FillBoard>> _fillBoard = default;
    private readonly EcsFilterInject<Inc<Components.GameBoard>> _board = default;

    private readonly EcsPoolInject<Components.GamePiece> _piecePool = default;
    private readonly EcsPoolInject<Components.DestroyPiece> _destroyPool = default;
    private readonly EcsPoolInject<Components.GamePieceType> _typePool = default;
    private readonly EcsPoolInject<Components.SolvePieceMatch> _solveMatch = default;

    private readonly EcsSharedInject<SharedData> _shared = default;
    private readonly EcsWorldInject _world = default;

    public void Run(IEcsSystems systems)
    {
        if (_fillBoard.Value.GetEntitiesCount() < 1)
            return;

        foreach (var boardEntity in _board.Value)
        {
            ref var gameBoard = ref _board.Pools.Inc1.Get(boardEntity);
            var tileSize = DrawLogic.DrawUtils.GetTileSize(_shared.Value.TilesAtlas);
            for (int row = gameBoard.Board.GetLength(0) - 1; row >= 0; row--)
            {
                for (int column = 0; column < gameBoard.Board.GetLength(1); column++)
                {
                    var piecePacked = gameBoard.Board[row, column];
                    if (piecePacked.Unpack(_world.Value, out var pieceEntity) && !_destroyPool.Value.Has(pieceEntity))
                        continue;

                    var newPiece = _world.Value.NewEntity();
                    var startPosition = DrawLogic.DrawUtils.GetTileScreenPosition(-5, column,
                        _shared.Value.GraphicsDevice,
                        tileSize, _shared.Value.BoardSize);
                    ref var piece = ref _piecePool.Value.Add(newPiece);
                    piece.BoardPosition.Column = column;
                    piece.BoardPosition.Row = row;
                    piece.Transform = new Transform2(startPosition);
                    piece.Radius = tileSize.X / 2f;

                    ref var type = ref _typePool.Value.Add(newPiece);
                    type.Type = GameUtils.GetRandomPiece();

                    var entityPacked = _world.Value.PackEntity(newPiece);
                    gameBoard.Board[row, column] = entityPacked;

                    var endPosition = DrawLogic.DrawUtils.GetTileScreenPosition(row, column,
                        _shared.Value.GraphicsDevice,
                        tileSize, _shared.Value.BoardSize);
                    _shared.Value.Tweener.TweenTo(
                        target: piece.Transform,
                        expression: t => t.Position,
                        toValue: endPosition,
                        duration: GameConfig.REARRANGE_ANIMATION_TIME);

                    _solveMatch.Value.Add(_world.Value.NewEntity()) = new Components.SolvePieceMatch
                    {
                        StartPiece = entityPacked,
                        WaitTime = GameConfig.REARRANGE_ANIMATION_TIME
                    };
                }
            }
        }

        foreach (var fillBoardEntity in _fillBoard.Value)
        {
            _fillBoard.Pools.Inc1.Del(fillBoardEntity);
        }
    }
}