using System.Diagnostics;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace MonoMatch3.Code.GameLogic.Systems;

public class SwapWithoutMatchTracker : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.SwapWithotuMatch>> _cantMatch = default;
    private readonly EcsFilterInject<Inc<Components.LastSwap>> _lastSwap = default;

    private readonly EcsPoolInject<Components.Selected> _selectedPool = default;
    private readonly EcsPoolInject<Components.SwapWith> _swapPool = default;

    private readonly EcsWorldInject _world = default;

    public void Run(IEcsSystems systems)
    {
        if (_cantMatch.Value.GetEntitiesCount() > 1)
        {
            foreach (var lastSwapEntity in _lastSwap.Value)
            {
                ref var lastSwap = ref _lastSwap.Pools.Inc1.Get(lastSwapEntity);
                if(!lastSwap.Selected.Unpack(_world.Value, out var selected))
                    return;
                if(!lastSwap.Swapped.Unpack(_world.Value, out var swapped))
                    return;

                _selectedPool.Value.Add(selected).IsUndo = true;
                _swapPool.Value.Add(swapped);
                Debug.WriteLine("Unswap detected");
            }
        }
        
        foreach (var cantMatchEntity in _cantMatch.Value)
        {
            _cantMatch.Pools.Inc1.Del(cantMatchEntity);
        }
    }
}