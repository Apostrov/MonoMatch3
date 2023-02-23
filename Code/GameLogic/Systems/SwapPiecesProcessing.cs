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
    private readonly EcsFilterInject<Inc<Components.LastSwap>> _lastSwap = default;

    private readonly EcsPoolInject<Components.SolvePieceMatch> _solveMatchPool = default;
    private readonly EcsPoolInject<Components.SwapWithoutMatch> _cantMatchPool = default;

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

                // swap
                (swap.BoardPosition, selected.BoardPosition) =
                    (selected.BoardPosition, swap.BoardPosition);
                var selectedPack = _world.Value.PackEntity(selectedEntity);
                var swapPack = _world.Value.PackEntity(swapEntity);
                if (!_selectedPiece.Pools.Inc2.Get(selectedEntity).IsUndo)
                    AddLastSwap(selectedPack, swapPack);

                foreach (var boardEntity in _gameBoard.Value)
                {
                    ref var board = ref _gameBoard.Pools.Inc1.Get(boardEntity);
                    board.Board[selected.BoardPosition.Row, selected.BoardPosition.Column] = selectedPack;
                    board.Board[swap.BoardPosition.Row, swap.BoardPosition.Column] = swapPack;
                }

                // swap animation
                _shared.Value.Tweener.TweenTo(target: selected.Transform, expression: t => t.Position,
                    toValue: swap.Transform.Position, duration: GameConfig.SWAP_ANIMATION_TIME);
                _shared.Value.Tweener.TweenTo(target: swap.Transform, expression: t => t.Position,
                        toValue: selected.Transform.Position, duration: GameConfig.SWAP_ANIMATION_TIME)
                    .OnEnd(_ => _swapPiece.Pools.Inc2.Del(swapEntity));

                if (_selectedPiece.Pools.Inc2.Get(selectedEntity).IsUndo)
                    continue;

                // try to match
                TryToMatch(selectedPack);
                TryToMatch(swapPack);
            }

            _selectedPiece.Pools.Inc2.Get(selectedEntity).AnimationTween?.Cancel();
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

    private void AddLastSwap(EcsPackedEntity selected, EcsPackedEntity swap)
    {
        foreach (var lastSwapIndex in _lastSwap.Value)
        {
            _lastSwap.Pools.Inc1.Del(lastSwapIndex);
        }

        _lastSwap.Pools.Inc1.Add(_world.Value.NewEntity()) = new Components.LastSwap
        {
            Selected = selected,
            Swapped = swap
        };
    }

    private void TryToMatch(EcsPackedEntity start)
    {
        _solveMatchPool.Value.Add(_world.Value.NewEntity()) = new Components.SolvePieceMatch
        {
            StartPiece = start,
            WaitTime = GameConfig.SWAP_ANIMATION_TIME,
            OnDontMatchCallback = Reswap,
            IsClicked = true
        };
    }

    private void Reswap()
    {
        _cantMatchPool.Value.Add(_world.Value.NewEntity());
    }
}