using Microsoft.Xna.Framework;

namespace MonoMatch3.Code.UI.Model;

public class Label
{
    public string Text { get; }
    public Vector2 Position { get; }

    public Label(string text, Vector2 position)
    {
        Text = text;
        Position = position;
    }
}