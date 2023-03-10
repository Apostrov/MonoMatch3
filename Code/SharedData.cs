using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tweening;

namespace MonoMatch3.Code;

public class SharedData
{
    // draw logic
    public GraphicsDevice GraphicsDevice;
    public SpriteBatch SpriteBatch;
    public Texture2D Background;
    public Texture2D TilesAtlas;
    public SpriteFont Font;

    // game logic
    public Tweener Tweener;
    public GameTime GameTime;
}