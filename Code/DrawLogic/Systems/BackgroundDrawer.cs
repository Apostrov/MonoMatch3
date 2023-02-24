using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;

namespace MonoMatch3.Code.DrawLogic.Systems;

public class BackgroundDrawer : IEcsRunSystem
{
    private readonly EcsSharedInject<SharedData> _shared = default;
    
    public void Run(IEcsSystems systems)
    {
        _shared.Value.SpriteBatch.Begin();
        _shared.Value.SpriteBatch.Draw(_shared.Value.Background, _shared.Value.GraphicsDevice.Viewport.Bounds, Color.White);
        _shared.Value.SpriteBatch.End();
    }
}