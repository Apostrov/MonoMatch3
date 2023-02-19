using System.Diagnostics;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoMatch3.Code.GameLogic.Systems;

public class PlayerClickedProcessing : IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter _piecePositions;

    private EcsPool<Components.GamePiecePosition> _positionPool;
    private EcsPool<Components.Clicked> _clickedPool;

    private EcsWorld _world;
    private SharedData _shared;

    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();
        _shared = systems.GetShared<SharedData>();

        _piecePositions = _world.Filter<Components.GamePiecePosition>().End();
        _positionPool = _world.GetPool<Components.GamePiecePosition>();
        _clickedPool = _world.GetPool<Components.Clicked>();
    }

    public void Run(IEcsSystems systems)
    {
        var mouseState = Mouse.GetState();
        if (mouseState.LeftButton != ButtonState.Pressed)
            return;

        var clickedPoint = new Point(mouseState.X, mouseState.Y);
        foreach (var pieceEntity in _piecePositions)
        {
            ref var position = ref _positionPool.Get(pieceEntity);
            if (position.DrawnPosition.Contains(clickedPoint))
            {
                if (_clickedPool.Has(pieceEntity))
                    continue;
                _clickedPool.Add(pieceEntity);
            }
        }
    }
}