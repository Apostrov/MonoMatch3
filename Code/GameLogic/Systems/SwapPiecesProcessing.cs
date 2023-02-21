using System;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;

namespace MonoMatch3.Code.GameLogic.Systems;

public class SwapPiecesProcessing : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.GameBoard>> _gameBoard = default;
    private readonly EcsFilterInject<Inc<Components.GamePiece, Components.Selected>> _selectedPiece = default;
    private readonly EcsFilterInject<Inc<Components.GamePiece, Components.SwapWith>> _swapPiece = default;

    private readonly EcsPoolInject<Components.SolvePieceMatch> _solveMatch = default;

    private readonly EcsSharedInject<SharedData> _shared = default;
    private readonly EcsWorldInject _world = default;

    public void Run(IEcsSystems systems)
    {
        if (_swapPiece.Value.GetEntitiesCount() < 1)
            return;

        foreach (var selectedEntity in _selectedPiece.Value)
        {
            ref var selected = ref _selectedPiece.Pools.Inc1.Get(selectedEntity);

            foreach (var swapEntity in _swapPiece.Value)
            {
                ref var swap = ref _swapPiece.Pools.Inc1.Get(swapEntity);
                if (!CanSwap(selected.BoardPosition, swap.BoardPosition))
                {
                    _swapPiece.Pools.Inc2.Del(swapEntity);
                    continue;
                }

                (swap.BoardPosition, selected.BoardPosition) =
                    (selected.BoardPosition, swap.BoardPosition);
                foreach (var boardEntity in _gameBoard.Value)
                {
                    ref var board = ref _gameBoard.Pools.Inc1.Get(boardEntity);
                    board.Board[selected.BoardPosition.Row, selected.BoardPosition.Column] =
                        _world.Value.PackEntity(selectedEntity);
                    board.Board[swap.BoardPosition.Row, swap.BoardPosition.Column] =
                        _world.Value.PackEntity(swapEntity);
                }

                _shared.Value.Tweener.TweenTo(target: selected.Transform, expression: t => t.Position,
                    toValue: swap.Transform.Position, duration: GameConfig.SWAP_ANIMATION_TIME);
                _shared.Value.Tweener.TweenTo(target: swap.Transform, expression: t => t.Position,
                        toValue: selected.Transform.Position, duration: GameConfig.SWAP_ANIMATION_TIME)
                    .OnEnd(_ => _swapPiece.Pools.Inc2.Del(swapEntity));

                _solveMatch.Value.Add(_world.Value.NewEntity()) = new Components.SolvePieceMatch
                {
                    StartPiece = _world.Value.PackEntity(selectedEntity),
                    WaitTime = GameConfig.SWAP_ANIMATION_TIME
                };
                _solveMatch.Value.Add(_world.Value.NewEntity()) = new Components.SolvePieceMatch
                {
                    StartPiece = _world.Value.PackEntity(swapEntity),
                    WaitTime = GameConfig.SWAP_ANIMATION_TIME
                };
            }

            _selectedPiece.Pools.Inc2.Get(selectedEntity).AnimationTween.Cancel();
            _selectedPiece.Pools.Inc2.Del(selectedEntity);
            selected.Transform.Scale = Vector2.One;
        }
    }

    private bool CanSwap(PiecePosition firstPiece, PiecePosition secondPiece)
    {
        if (Math.Abs(firstPiece.Row - secondPiece.Row) == 0)
            return Math.Abs(firstPiece.Column - secondPiece.Column) == 1;

        return Math.Abs(firstPiece.Column - secondPiece.Column) == 0 && Math.Abs(firstPiece.Row - secondPiece.Row) == 1;
    }
}