using Leopotam.EcsLite;
using Microsoft.Xna.Framework;

namespace MonoMatch3.Code.DrawLogic.Systems;

public class BackgroundDrawer : IEcsInitSystem, IEcsRunSystem
{
    private SharedData _shared;

    public void Init(IEcsSystems systems)
    {
        _shared = systems.GetShared<SharedData>();
    }

    public void Run(IEcsSystems systems)
    {
        _shared.SpriteBatch.Begin();
        _shared.SpriteBatch.Draw(_shared.Background, _shared.GraphicsDevice.Viewport.Bounds, Color.White);
        _shared.SpriteBatch.End();
    }
}