using System.Diagnostics;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoMatch3.Code.GameLogic.Systems;

public class PlayerClickedProcessing : IEcsInitSystem, IEcsRunSystem
{
    private EcsFilter _piecePositions;

    private EcsPool<Components.GamePiece> _piecePool;
    private EcsPool<Components.Clicked> _clickedPool;

    private EcsWorld _world;
    private SharedData _shared;

    public void Init(IEcsSystems systems)
    {
        _world = systems.GetWorld();
        _shared = systems.GetShared<SharedData>();

        _piecePositions = _world.Filter<Components.GamePiece>().End();
        _piecePool = _world.GetPool<Components.GamePiece>();
        _clickedPool = _world.GetPool<Components.Clicked>();
    }

    public void Run(IEcsSystems systems)
    {
        var mouseState = Mouse.GetState();
        if (mouseState.LeftButton != ButtonState.Pressed)
            return;

        var mousePosition = new Vector2(mouseState.X, mouseState.Y);
        foreach (var pieceEntity in _piecePositions)
        {
            ref var gamePiece = ref _piecePool.Get(pieceEntity);
            if (Vector2.DistanceSquared(mousePosition, gamePiece.Transform.Position) <
                gamePiece.Radius * gamePiece.Radius)
            {
                if (_clickedPool.Has(pieceEntity))
                    continue;
                _clickedPool.Add(pieceEntity);
            }
        }
    }
}