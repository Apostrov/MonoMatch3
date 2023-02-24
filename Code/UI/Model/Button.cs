using Microsoft.Xna.Framework;

namespace MonoMatch3.Code.UI.Model;

public class Button
{
    public string Text { get; }
    public Rectangle Bounds { get; }
    public Vector2 TextPosition { get; }

    public Button(string text, Rectangle bounds, Vector2 textPosition)
    {
        Text = text;
        Bounds = bounds;
        TextPosition = textPosition;
    }
}