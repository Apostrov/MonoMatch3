using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input;
using MonoGame.Extended.Tweening;

namespace MonoMatch3.Code.GameLogic.Systems;

public class PlayerClickedProcessing : IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter _pieces;
    private EcsFilter _selected;
    private EcsFilter _swapWait;

    private EcsPool<Components.GamePiece> _piecePool;
    private EcsPool<Components.Selected> _selectedPool;
    private EcsPool<Components.SwapWith> _swap;

    private EcsWorld _world;
    private SharedData _shared;

    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();
        _shared = systems.GetShared<SharedData>();

        _pieces = _world.Filter<Components.GamePiece>().End();
        _selected = _world.Filter<Components.Selected>().End();
        _swapWait = _world.Filter<Components.SwapWith>().End();

        _piecePool = _world.GetPool<Components.GamePiece>();
        _selectedPool = _world.GetPool<Components.Selected>();
        _swap = _world.GetPool<Components.SwapWith>();
    }

    public void Run(IEcsSystems systems)
    {
        if (_swapWait.GetEntitiesCount() > 0)
            return;

        var mouseState = MouseExtended.GetState();
        if (!mouseState.WasButtonJustUp(MouseButton.Left))
            return;

        var mousePosition = new Vector2(mouseState.X, mouseState.Y);
        foreach (var pieceEntity in _pieces)
        {
            ref var gamePiece = ref _piecePool.Get(pieceEntity);
            if (Vector2.DistanceSquared(mousePosition, gamePiece.Transform.Position) <
                gamePiece.Radius * gamePiece.Radius)
            {
                if (_selected.GetEntitiesCount() > 0)
                {
                    if (_selectedPool.Has(pieceEntity))
                    {
                        _selectedPool.Get(pieceEntity).AnimationTween.Cancel();
                        _selectedPool.Del(pieceEntity);
                        gamePiece.Transform.Scale = Vector2.One;
                        continue;
                    }

                    _swap.Add(pieceEntity);
                    continue;
                }

                ref var selected = ref _selectedPool.Add(pieceEntity);
                selected.AnimationTween = _shared.Tweener.TweenTo(target: gamePiece.Transform,
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