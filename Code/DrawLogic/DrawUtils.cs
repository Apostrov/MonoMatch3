using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoMatch3.Code.DrawLogic;

public static class DrawUtils
{
    public const int ATLAS_SIZE = 7;
    public const int TILES_ROW = 0;
    public const int BACKGROUND_ROW = 5;
    public const int BONUSES_ROW = 4;

    public static Point GetTileSize(Texture2D atlas)
    {
        return new Point(atlas.Width / ATLAS_SIZE, atlas.Height / ATLAS_SIZE);
    }

    public static Vector2 GetTileScreenPosition(int row, int column, GraphicsDevice graphicsDevice, Point tileSize,
        int boardSize)
    {
        int centerX = graphicsDevice.Viewport.Width / 2 + tileSize.X / 2;
        int centerY = graphicsDevice.Viewport.Height / 2 + tileSize.Y / 2;
        int startX = centerX - tileSize.X * boardSize / 2;
        int startY = centerY - tileSize.Y * boardSize / 2;
        return new Vector2(startX + column * tileSize.X, startY + row * tileSize.Y);
    }
}