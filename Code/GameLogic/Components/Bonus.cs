using System.Collections.Generic;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Tweening;

namespace MonoMatch3.Code.GameLogic.Components;

public struct Bonus
{
}

public struct BonusMatch
{
}

public struct BonusSpawn
{
    public int Destroyed;
    public PiecePosition Position;
    public float WaitTime;
}

public struct Line
{
    public LineType Type;
}

public struct LineDestroyer
{
    public LineDestroyerType Type;
    public Vector2 FlyPosition;
    public Queue<(Vector2 position, EcsPackedEntity packedEntity)> FlyPoints;
    public Transform2 Transform;
}

public struct Bomb
{
}

public struct BombExplosion
{
    public float WaitTime;
    public Tween Animation;
}