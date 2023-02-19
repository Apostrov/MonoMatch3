﻿using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tweening;

namespace MonoMatch3.Code;

public class SharedData
{
    // draw logic
    public GraphicsDevice GraphicsDevice;
    public SpriteBatch SpriteBatch;
    public Texture2D Background;
    public Texture2D TilesAtlas;
    public Sprite Sprite;
    
    // game logic
    public Tweener Tweener;
}