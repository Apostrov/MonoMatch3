using System;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;

namespace MonoMatch3.Code.GameLogic.Systems;

public class SwapPiecesProcessing : IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter _selectedPiece;
    private EcsFilter _swapPiece;

    private EcsPool<Components.GamePiece> _piecePool;
    private EcsPool<Components.Selected> _selectedPool;
    private EcsPool<Components.SwapWith> _swapPool;

    private EcsWorld _world;
    private SharedData _shared;

    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();
        _shared = systems.GetShared<SharedData>();

        _selectedPiece = _world.Filter<Components.GamePiece>().Inc<Components.Selected>().End();
        _swapPiece = _world.Filter<Components.GamePiece>().Inc<Components.SwapWith>().End();

        _piecePool = _world.GetPool<Components.GamePiece>();
        _selectedPool = _world.GetPool<Components.Selected>();
        _swapPool = _world.GetPool<Components.SwapWith>();
    }

    public void Run(IEcsSystems systems)
    {
        if (_swapPiece.GetEntitiesCount() < 1)
            return;

        foreach (var selectedEntity in _selectedPiece)
        {
            ref var firstPiece = ref _piecePool.Get(selectedEntity);

            foreach (var swapEntity in _swapPiece)
            {
                ref var secondPiece = ref _piecePool.Get(swapEntity);
                if (!CanSwap(firstPiece.Row, firstPiece.Column, secondPiece.Row, secondPiece.Column))
                {
                    _swapPool.Del(swapEntity);
                    continue;
                }
                (secondPiece.Row, firstPiece.Row) = (firstPiece.Row, secondPiece.Row);
                (secondPiece.Column, firstPiece.Column) = (firstPiece.Column, secondPiece.Column);
                
                _shared.Tweener.TweenTo(target: secondPiece.Transform, expression: t => t.Position,
                        toValue: firstPiece.Transform.Position, duration: GameConfig.SWAP_ANIMATION_TIME)
                    .OnEnd(_ => _swapPool.Del(swapEntity));
                _shared.Tweener.TweenTo(target: firstPiece.Transform, expression: t => t.Position,
                    toValue: secondPiece.Transform.Position, duration: GameConfig.SWAP_ANIMATION_TIME);
            }

            _selectedPool.Get(selectedEntity).AnimationTween.Cancel();
            _selectedPool.Del(selectedEntity);
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