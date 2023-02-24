using Leopotam.EcsLite;
using MonoMatch3.Code.GameLogic.Components;

namespace MonoMatch3.Code.GameLogic.Algorithms;

public class SimpleDFS : DFS
{
    public SimpleDFS(EcsPool<GamePiece> piecePool, EcsWorld world) : base(piecePool, world)
    {
    }

    protected override bool IsCorrect(int startPieceEntity, int currentPieceEntity)
    {
        return true;
    }
}