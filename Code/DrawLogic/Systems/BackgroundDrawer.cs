using Leopotam.EcsLite;
using Microsoft.Xna.Framework;

namespace MonoMatch3.Code.DrawLogic.Systems;

public class BackgroundDrawer : IEcsInitSystem, IEcsRunSystem
{
    private SharedData _sharedData;

    public void Init(IEcsSystems systems)
    {
        _sharedData = systems.GetShared<SharedData>();
    }

    public void Run(IEcsSystems systems)
    {
        _sharedData.SpriteBatch.Begin();
        _sharedData.SpriteBatch.Draw(_sharedData.Background, _sharedData.GraphicsDevice.Viewport.Bounds, Color.White);
        _sharedData.SpriteBatch.End();
    }
}