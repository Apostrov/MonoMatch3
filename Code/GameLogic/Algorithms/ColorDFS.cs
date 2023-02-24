using Leopotam.EcsLite;
using MonoMatch3.Code.GameLogic.Components;

namespace MonoMatch3.Code.GameLogic.Algorithms;

public class ColorDFS : DFS
{
    private readonly EcsPool<GamePieceType> _typePool;

    public ColorDFS(EcsPool<GamePiece> piecePool, EcsWorld world, EcsPool<GamePieceType> typePool) : base(piecePool,
        world)
    {
        _typePool = typePool;
    }

    protected override bool IsCorrect(int startPieceEntity, int currentPieceEntity)
    {
        return _typePool.Has(currentPieceEntity) &&
               _typePool.Get(currentPieceEntity).Type == _typePool.Get(startPieceEntity).Type;
    }
}