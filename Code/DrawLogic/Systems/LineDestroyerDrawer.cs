using System.Collections.Generic;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;

namespace MonoMatch3.Code.DrawLogic.Systems;

public class LineDestroyerDrawer : IEcsInitSystem, IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<GameLogic.Components.LineDestroyer>> _destroyer = default;

    private readonly EcsSharedInject<SharedData> _shared = default;

    private Point _tileSize;
    private Sprite _destroyerSprite;

    public void Init(IEcsSystems systems)
    {
        // atlas related 
        _tileSize = DrawUtils.GetTileSize(_shared.Value.TilesAtlas);
        var rect = new Rectangle(0, _tileSize.Y * DrawUtils.BONUSES_ROW, _tileSize.X / 2, _tileSize.Y);
        _destroyerSprite = new Sprite(new TextureRegion2D(_shared.Value.TilesAtlas, rect));
    }

    public void Run(IEcsSystems systems)
    {
        _shared.Value.SpriteBatch.Begin();

        foreach (var destroyerEntity in _destroyer.Value)
        {
            ref var destroyer = ref _destroyer.Pools.Inc1.Get(destroyerEntity);
            _shared.Value.SpriteBatch.Draw(_destroyerSprite, destroyer.Transform);
        }

        _shared.Value.SpriteBatch.End();
    }
}