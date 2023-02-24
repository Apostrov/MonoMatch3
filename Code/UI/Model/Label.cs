using Microsoft.Xna.Framework;

namespace MonoMatch3.Code.UI.Model;

public class Label
{
    public string Text { get; set; }
    public Vector2 Position { get; set; }

    public Label(string text, Vector2 position)
    {
        Text = text;
        Position = position;
    }
}