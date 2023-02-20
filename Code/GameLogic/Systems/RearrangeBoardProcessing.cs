using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class RearrangeBoardProcessing : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.RearrangeBoard>> _rearrange = default;
    private readonly EcsFilterInject<Inc<Components.GameBoard>> _board = default;

    private readonly EcsPoolInject<Components.GamePiece> _piecePool = default;
    private readonly EcsPoolInject<Components.DestroyPiece> _destroyPool = default;
    private readonly EcsPoolInject<Components.RearrangePiece> _rearrangePiecePool = default;

    private readonly EcsWorldInject _world = default;
    private readonly EcsSharedInject<SharedData> _shared = default;

    public void Run(IEcsSystems systems)
    {
        foreach (var rearrangeEntity in _rearrange.Value)
        {
            ref var rearrange = ref _rearrange.Pools.Inc1.Get(rearrangeEntity);
            rearrange.WaitTime -= _shared.Value.GameTime.GetElapsedSeconds();
            if (rearrange.WaitTime > 0.0f)
                continue;
            _rearrange.Pools.Inc1.Del(rearrangeEntity);
        }

        if (_rearrange.Value.GetEntitiesCount() > 0)
            return;

        foreach (var boardEntity in _board.Value)
        {
            var board = _board.Pools.Inc1.Get(boardEntity).Board;
            var tileSize = DrawLogic.DrawUtils.GetTileSize(_shared.Value.TilesAtlas);
            for (int i = board.GetLength(0) - 1; i >= 0; i--)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    var piecePacked = board[i, j];
                    if (piecePacked.Unpack(_world.Value, out var pieceEntity) && !_destroyPool.Value.Has(pieceEntity))
                        continue;

                    var nextIndex = 1;
                    while (i - nextIndex >= 0)
                    {
                        var newI = i - nextIndex;
                        var nextPiecePacked = board[newI, j];
                        nextIndex++;
                        if (!nextPiecePacked.Unpack(_world.Value, out var nextPieceEntity) ||
                            _destroyPool.Value.Has(nextPieceEntity))
                            continue;

                        (board[i, j], board[newI, j]) = (board[newI, j], board[i, j]);
                        ref var nextPiece = ref _piecePool.Value.Get(nextPieceEntity);
                        nextPiece.BoardPosition = new PiecePosition(i, j);
                        var position = DrawLogic.DrawUtils.GetTileScreenPosition(i, j,
                            _shared.Value.GraphicsDevice, tileSize, _shared.Value.BoardSize);

                        if (_rearrangePiecePool.Value.Has(nextPieceEntity))
                        {
                            _rearrangePiecePool.Value.Get(nextPieceEntity).Animation.Cancel();
                            _rearrangePiecePool.Value.Del(nextPieceEntity);
                        }

                        _rearrangePiecePool.Value.Add(nextPieceEntity).Animation = _shared.Value.Tweener.TweenTo(
                            target: nextPiece.Transform,
                            expression: t => t.Position,
                            toValue: position,
                            duration: GameConfig.REARRANGE_ANIMATION_TIME);
                        break;
                    }
                }
            }
        }
    }
}