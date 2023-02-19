using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using MonoGame.Extended.Tweening;

namespace MonoMatch3.Code.GameLogic.Systems;

public class PlayerClickedProcessing : IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter _piecePositions;

    private EcsPool<Components.GamePiece> _piecePool;
    private EcsPool<Components.Selected> _selectedPool;

    private EcsWorld _world;
    private SharedData _shared;

    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();
        _shared = systems.GetShared<SharedData>();

        _piecePositions = _world.Filter<Components.GamePiece>().End();
        _piecePool = _world.GetPool<Components.GamePiece>();
        _selectedPool = _world.GetPool<Components.Selected>();
    }

    public void Run(IEcsSystems systems)
    {
        var mouseState = MouseExtended.GetState();
        if (!mouseState.WasButtonJustUp(MouseButton.Left))
            return;

        var mousePosition = new Vector2(mouseState.X, mouseState.Y);
        foreach (var pieceEntity in _piecePositions)
        {
            ref var gamePiece = ref _piecePool.Get(pieceEntity);
            if (Vector2.DistanceSquared(mousePosition, gamePiece.Transform.Position) <
                gamePiece.Radius * gamePiece.Radius)
            {
                if (_selectedPool.Has(pieceEntity))
                {
                    _selectedPool.Get(pieceEntity).AnimationTween.Cancel();
                    _selectedPool.Del(pieceEntity);
                    gamePiece.Transform.Scale = Vector2.One;
                    continue;
                }

                var transform = gamePiece.Transform;
                _selectedPool.Add(pieceEntity).AnimationTween = _shared.Tweener.TweenTo(target: transform,
                            expression: t => t.Scale,
                            toValue: new Vector2(0.6f, 0.6f), duration: 0.75f)
                        .RepeatForever()
                        .AutoReverse()
                        .Easing(EasingFunctions.Linear)
                    ;
            }
        }
    }
}