using System;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;

namespace MonoMatch3.Code.GameLogic.Systems;

public class SwapPiecesProcessing : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.GamePiece, Components.Selected>> _selectedPiece = default;
    private readonly EcsFilterInject<Inc<Components.GamePiece, Components.SwapWith>> _swapPiece = default;

    private readonly EcsSharedInject<SharedData> _shared = default;
    private readonly EcsWorldInject _world = default;

    public void Run(IEcsSystems systems)
    {
        if (_swapPiece.Value.GetEntitiesCount() < 1)
            return;

        foreach (var selectedEntity in _selectedPiece.Value)
        {
            ref var firstPiece = ref _selectedPiece.Pools.Inc1.Get(selectedEntity);

            foreach (var swapEntity in _swapPiece.Value)
            {
                ref var secondPiece = ref _swapPiece.Pools.Inc1.Get(swapEntity);
                if (!CanSwap(firstPiece.BoardPosition, secondPiece.BoardPosition))
                {
                    _swapPiece.Pools.Inc2.Del(swapEntity);
                    continue;
                }

                (secondPiece.BoardPosition, firstPiece.BoardPosition) =
                    (firstPiece.BoardPosition, secondPiece.BoardPosition);

                _shared.Value.Tweener.TweenTo(target: secondPiece.Transform, expression: t => t.Position,
                        toValue: firstPiece.Transform.Position, duration: GameConfig.SWAP_ANIMATION_TIME)
                    .OnEnd(_ => _swapPiece.Pools.Inc2.Del(swapEntity));
                _shared.Value.Tweener.TweenTo(target: firstPiece.Transform, expression: t => t.Position,
                    toValue: secondPiece.Transform.Position, duration: GameConfig.SWAP_ANIMATION_TIME);
            }

            _selectedPiece.Pools.Inc2.Get(selectedEntity).AnimationTween.Cancel();
            _selectedPiece.Pools.Inc2.Del(selectedEntity);
            firstPiece.Transform.Scale = Vector2.One;
        }
    }

    private bool CanSwap(PiecePosition firstPiece, PiecePosition secondPiece)
    {
        if (Math.Abs(firstPiece.Row - secondPiece.Row) == 0)
            return Math.Abs(firstPiece.Column - secondPiece.Column) == 1;

        return Math.Abs(firstPiece.Column - secondPiece.Column) == 0 && Math.Abs(firstPiece.Row - secondPiece.Row) == 1;
    }
}