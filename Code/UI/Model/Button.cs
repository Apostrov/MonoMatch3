using Microsoft.Xna.Framework;

namespace MonoMatch3.Code.UI.Model;

public class Button
{
    public string Text { get; set; }
    public Rectangle Bounds { get; set; }
    public Vector2 TextPosition { get; set; }

    public Button(string text, Rectangle bounds, Vector2 textPosition)
    {
        Text = text;
        Bounds = bounds;
        TextPosition = textPosition;
    }
}