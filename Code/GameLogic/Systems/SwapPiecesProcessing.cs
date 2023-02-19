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
                (secondPiece.Transform.Position, firstPiece.Transform.Position) =
                    (firstPiece.Transform.Position, secondPiece.Transform.Position);
                _swapPool.Del(swapEntity);
            }

            _selectedPool.Get(selectedEntity).AnimationTween.Cancel();
            _selectedPool.Del(selectedEntity);
            firstPiece.Transform.Scale = Vector2.One;
        }
    }
}