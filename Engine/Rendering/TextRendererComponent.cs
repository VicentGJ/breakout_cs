using System.Numerics;
using Raylib_cs;

namespace Breakout;

sealed class TextRendererComponent : Component
{
    public string Text = string.Empty;
    public int FontSize = 16;
    public Vector2 Offset = Vector2.Zero;
    public Color Color = Color.White;
}
