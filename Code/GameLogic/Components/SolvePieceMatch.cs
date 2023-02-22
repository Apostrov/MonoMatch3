using System;
using Leopotam.EcsLite;

namespace MonoMatch3.Code.GameLogic.Components;

public struct SolvePieceMatch
{
    public EcsPackedEntity StartPiece;
    public float WaitTime;
    public Action OnDontMatchCallback;
}