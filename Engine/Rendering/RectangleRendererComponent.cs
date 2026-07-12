using System.Numerics;
using Raylib_cs;

namespace Breakout;

sealed class RectangleRendererComponent : Component
{
    public Vector2 Size;
    public Vector2 Offset = Vector2.Zero;
    public Vector2 Origin = Vector2.Zero;
    public Color FillColor;
    public Color? StrokeColor = null;
}
