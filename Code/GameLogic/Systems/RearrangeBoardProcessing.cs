using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class RearrangeBoardProcessing : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.RearrangeBoard>> _rearrange = default;
    private readonly EcsFilterInject<Inc<Components.GameBoard>> _board = default;
    private readonly EcsFilterInject<Inc<Components.DestroyPiece>> _destroy = default;
    private readonly EcsFilterInject<Inc<Components.LineDestroyer>> _destroyer = default;
    private readonly EcsFilterInject<Inc<Components.BombExplosion>> _explosion = default;

    private readonly EcsPoolInject<Components.GamePiece> _piecePool = default;
    private readonly EcsPoolInject<Components.RearrangePiece> _rearrangePiecePool = default;
    private readonly EcsPoolInject<Components.FillBoard> _fillBoard = default;
    private readonly EcsPoolInject<Components.SolvePieceMatch> _solveMatch = default;

    private readonly EcsWorldInject _world = default;
    private readonly EcsSharedInject<SharedData> _shared = default;

    public void Run(IEcsSystems systems)
    {
        if (_rearrange.Value.GetEntitiesCount() < 1)
            return;

        foreach (var rearrangeEntity in _rearrange.Value)
        {
            ref var rearrange = ref _rearrange.Pools.Inc1.Get(rearrangeEntity);
            rearrange.WaitTime -= _shared.Value.GameTime.GetElapsedSeconds();
            if (rearrange.WaitTime > 0.0f ||
                _destroy.Value.GetEntitiesCount() > 0 ||
                _destroyer.Value.GetEntitiesCount() > 0 ||
                _explosion.Value.GetEntitiesCount() > 0)
                continue;
            _rearrange.Pools.Inc1.Del(rearrangeEntity);
        }

        if (_rearrange.Value.GetEntitiesCount() > 0)
            return;

        foreach (var boardEntity in _board.Value)
        {
            var board = _board.Pools.Inc1.Get(boardEntity).Board;
            var tileSize = DrawLogic.DrawUtils.GetTileSize(_shared.Value.TilesAtlas);
            for (int row = board.GetLength(0) - 1; row >= 0; row--)
            {
                for (int column = 0; column < board.GetLength(1); column++)
                {
                    var piecePacked = board[row, column];
                    if (piecePacked.Unpack(_world.Value, out var pieceEntity) && !_destroy.Pools.Inc1.Has(pieceEntity))
                        continue;

                    var moveRow = 1;
                    while (row - moveRow >= 0)
                    {
                        var newRow = row - moveRow;
                        var nextPiecePacked = board[newRow, column];
                        moveRow++;
                        if (!nextPiecePacked.Unpack(_world.Value, out var nextPieceEntity) ||
                            _destroy.Pools.Inc1.Has(nextPieceEntity))
                            continue;

                        (board[row, column], board[newRow, column]) = (board[newRow, column], board[row, column]);
                        ref var nextPiece = ref _piecePool.Value.Get(nextPieceEntity);
                        nextPiece.BoardPosition = new PiecePosition(row, column);
                        var position = DrawLogic.DrawUtils.GetTileScreenPosition(row, column,
                            _shared.Value.GraphicsDevice, tileSize);

                        if (_rearrangePiecePool.Value.Has(nextPieceEntity))
                        {
                            _rearrangePiecePool.Value.Get(nextPieceEntity).Animation.Cancel();
                            _rearrangePiecePool.Value.Del(nextPieceEntity);
                        }

                        _rearrangePiecePool.Value.Add(nextPieceEntity).Animation = _shared.Value.Tweener.TweenTo(
                                target: nextPiece.Transform,
                                expression: t => t.Position,
                                toValue: position,
                                duration: GameConfig.REARRANGE_ANIMATION_TIME)
                            .OnEnd(_ => _rearrangePiecePool.Value.Del(nextPieceEntity));

                        _solveMatch.Value.Add(_world.Value.NewEntity()) = new Components.SolvePieceMatch
                        {
                            StartPiece = nextPiecePacked,
                            WaitTime = GameConfig.REARRANGE_ANIMATION_TIME
                        };
                        break;
                    }
                }
            }

            _fillBoard.Value.Add(_world.Value.NewEntity());
        }
    }
}