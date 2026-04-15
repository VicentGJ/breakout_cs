using System.Numerics;
using Raylib_cs;

namespace Breakout;

internal struct TextRendererComponent : IComponent
{
    public string Text = string.Empty;
    public int FontSize = 16;
    public Vector2 Offset = Vector2.Zero;
    public Color Color = Color.White;

    public TextRendererComponent()
    {
    }
}
