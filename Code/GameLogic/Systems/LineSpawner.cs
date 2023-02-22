using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using MonoGame.Extended;

namespace MonoMatch3.Code.GameLogic.Systems;

public class LineSpawner : IEcsRunSystem
{
    private readonly EcsFilterInject<Inc<Components.LineSpawn>> _lineSpawn = default;
    private readonly EcsFilterInject<Inc<Components.GameBoard>> _gameBoard = default;
    private readonly EcsFilterInject<Inc<Components.LastSwap>> _lastSwap = default;

    private readonly EcsPoolInject<Components.GamePiece> _piecePool = default;
    private readonly EcsPoolInject<Components.Line> _linePool = default;

    private readonly EcsSharedInject<SharedData> _shared = default;
    private readonly EcsWorldInject _world = default;

    public void Run(IEcsSystems systems)
    {
        if (_lineSpawn.Value.GetEntitiesCount() < 1)
            return;

        foreach (var lineSpawnEntity in _lineSpawn.Value)
        {
            ref var lineSpawn = ref _lineSpawn.Pools.Inc1.Get(lineSpawnEntity);
            lineSpawn.WaitTime -= _shared.Value.GameTime.GetElapsedSeconds();
            if (lineSpawn.WaitTime <= 0.0f)
            {
                foreach (var gameBoardEntity in _gameBoard.Value)
                {
                    ref var gameBoard = ref _gameBoard.Pools.Inc1.Get(gameBoardEntity);
                    int row = lineSpawn.LinePosition.Row, column = lineSpawn.LinePosition.Column;
                    var tileSize = DrawLogic.DrawUtils.GetTileSize(_shared.Value.TilesAtlas);
                    var position = DrawLogic.DrawUtils.GetTileScreenPosition(row, column, _shared.Value.GraphicsDevice,
                        tileSize, _shared.Value.BoardSize);

                    var pieceEntity = _world.Value.NewEntity();
                    ref var piece = ref _piecePool.Value.Add(pieceEntity);
                    piece.BoardPosition.Column = column;
                    piece.BoardPosition.Row = row;
                    piece.Transform = new Transform2(position);
                    piece.Radius = tileSize.X / 2f;
                    
                    _linePool.Value.Add(pieceEntity).Type = GetLineType();

                    var entityPacked = _world.Value.PackEntity(pieceEntity);
                    gameBoard.Board[row, column] = entityPacked;
                }

                _lineSpawn.Pools.Inc1.Del(lineSpawnEntity);
            }
        }
    }

    private LineType GetLineType()
    {
        foreach (var lastSwapEntity in _lastSwap.Value)
        {
            ref var lastSwap = ref _lastSwap.Pools.Inc1.Get(lastSwapEntity);
            if(!lastSwap.Selected.Unpack(_world.Value, out var selectedEntity))
                return LineType.Row;
            if(!lastSwap.Swapped.Unpack(_world.Value, out var swappedEntity))
                return LineType.Row;

            ref var selected = ref _piecePool.Value.Get(selectedEntity);
            ref var swapped = ref _piecePool.Value.Get(swappedEntity);
            if (selected.BoardPosition.Column == swapped.BoardPosition.Column)
                return LineType.Column;
        }
        return LineType.Row;
    }
}