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

    private readonly Dictionary<GameLogic.LineDestroyerType, Sprite> _line = new();
    private Point _tileSize;


    public void Init(IEcsSystems systems)
    {
        // atlas related 
        _tileSize = DrawUtils.GetTileSize(_shared.Value.TilesAtlas);
        for (int i = 0; i < (int)GameLogic.LineDestroyerType.Left; i++)
        {
            var halfWidth = _tileSize.X / 2;
            var rect = new Rectangle(i * halfWidth, _tileSize.Y * DrawUtils.BONUSES_ROW, halfWidth, _tileSize.Y);
            _line[(GameLogic.LineDestroyerType)i] = new Sprite(new TextureRegion2D(_shared.Value.TilesAtlas, rect));
        }

        for (int i = (int)GameLogic.LineDestroyerType.Left; i < (int)GameLogic.LineDestroyerType.COUNT; i++)
        {
            var halfHeight = _tileSize.Y / 2;
            var offset = i - (int)GameLogic.LineDestroyerType.Left;
            var rect = new Rectangle(_tileSize.X, _tileSize.Y * DrawUtils.BONUSES_ROW + offset * halfHeight,
                _tileSize.X, halfHeight);
            _line[(GameLogic.LineDestroyerType)i] = new Sprite(new TextureRegion2D(_shared.Value.TilesAtlas, rect));
        }
    }

    public void Run(IEcsSystems systems)
    {
        _shared.Value.SpriteBatch.Begin();

        foreach (var destroyerEntity in _destroyer.Value)
        {
            ref var destroyer = ref _destroyer.Pools.Inc1.Get(destroyerEntity);
            _shared.Value.SpriteBatch.Draw(_line[destroyer.Type], destroyer.Transform);
        }

        _shared.Value.SpriteBatch.End();
    }
}