using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input;
using MonoGame.Extended.Tweening;

namespace MonoMatch3.Code.GameLogic.Systems;

public class PlayerClickedProcessing : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.GamePiece>, Exc<Components.DestroyPiece>> _pieces = default;
    private readonly EcsFilterInject<Inc<Components.Selected>> _selected = default;
    private readonly EcsFilterInject<Inc<Components.SwapWith>> _swapWait = default;

    private readonly EcsPoolInject<Components.GamePiece> _piecePool = default;
    private readonly EcsPoolInject<Components.Selected> _selectedPool = default;
    private readonly EcsPoolInject<Components.SwapWith> _swap = default;

    private readonly EcsSharedInject<SharedData> _shared = default;
    private readonly EcsWorldInject _world = default;

    public void Run(IEcsSystems systems)
    {
        if (_swapWait.Value.GetEntitiesCount() > 0)
            return;

        var mouseState = MouseExtended.GetState();
        if (!mouseState.WasButtonJustUp(MouseButton.Left))
            return;

        var mousePosition = new Vector2(mouseState.X, mouseState.Y);
        foreach (var pieceEntity in _pieces.Value)
        {
            ref var gamePiece = ref _piecePool.Value.Get(pieceEntity);
            if (Vector2.DistanceSquared(mousePosition, gamePiece.Transform.Position) <
                gamePiece.Radius * gamePiece.Radius)
            {
                if (_selected.Value.GetEntitiesCount() > 0)
                {
                    if (_selectedPool.Value.Has(pieceEntity))
                    {
                        _selectedPool.Value.Get(pieceEntity).AnimationTween.Cancel();
                        _selectedPool.Value.Del(pieceEntity);
                        gamePiece.Transform.Scale = Vector2.One;
                        continue;
                    }

                    _swap.Value.Add(pieceEntity);
                    continue;
                }

                ref var selected = ref _selectedPool.Value.Add(pieceEntity);
                selected.AnimationTween = _shared.Value.Tweener.TweenTo(target: gamePiece.Transform,
                            expression: t => t.Scale,
                            toValue: new Vector2(GameConfig.SELECTED_ANIMATION_SHRINK,
                                GameConfig.SELECTED_ANIMATION_SHRINK),
                            duration: GameConfig.SELECTED_ANIMATION_TIME)
                        .RepeatForever()
                        .AutoReverse()
                        .Easing(EasingFunctions.Linear)
                    ;
            }
        }
    }
}