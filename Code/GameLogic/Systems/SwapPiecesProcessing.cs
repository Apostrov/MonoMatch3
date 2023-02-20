using System;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;

namespace MonoMatch3.Code.GameLogic.Systems;

public class SwapPiecesProcessing : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.GamePiece, Components.Selected>> _selectedPiece = default;
    private readonly EcsFilterInject<Inc<Components.GamePiece, Components.SwapWith>> _swapPiece = default;

    private readonly EcsPoolInject<Components.GamePiece> _piecePool = default;
    private readonly EcsPoolInject<Components.Selected> _selectedPool = default;
    private readonly EcsPoolInject<Components.SwapWith> _swapPool = default;

    private readonly EcsSharedInject<SharedData> _shared = default;
    private readonly EcsWorldInject _world = default;

    public void Run(IEcsSystems systems)
    {
        if (_swapPiece.Value.GetEntitiesCount() < 1)
            return;

        foreach (var selectedEntity in _selectedPiece.Value)
        {
            ref var firstPiece = ref _piecePool.Value.Get(selectedEntity);

            foreach (var swapEntity in _swapPiece.Value)
            {
                ref var secondPiece = ref _piecePool.Value.Get(swapEntity);
                if (!CanSwap(firstPiece.Row, firstPiece.Column, secondPiece.Row, secondPiece.Column))
                {
                    _swapPool.Value.Del(swapEntity);
                    continue;
                }

                (secondPiece.Row, firstPiece.Row) = (firstPiece.Row, secondPiece.Row);
                (secondPiece.Column, firstPiece.Column) = (firstPiece.Column, secondPiece.Column);

                _shared.Value.Tweener.TweenTo(target: secondPiece.Transform, expression: t => t.Position,
                        toValue: firstPiece.Transform.Position, duration: GameConfig.SWAP_ANIMATION_TIME)
                    .OnEnd(_ => _swapPool.Value.Del(swapEntity));
                _shared.Value.Tweener.TweenTo(target: firstPiece.Transform, expression: t => t.Position,
                    toValue: secondPiece.Transform.Position, duration: GameConfig.SWAP_ANIMATION_TIME);
            }

            _selectedPool.Value.Get(selectedEntity).AnimationTween.Cancel();
            _selectedPool.Value.Del(selectedEntity);
            firstPiece.Transform.Scale = Vector2.One;
        }
    }

    private bool CanSwap(int row1, int column1, int row2, int column2)
    {
        if (Math.Abs(row1 - row2) == 0)
            return Math.Abs(column1 - column2) == 1;

        return Math.Abs(column1 - column2) == 0 && Math.Abs(row1 - row2) == 1;
    }
}