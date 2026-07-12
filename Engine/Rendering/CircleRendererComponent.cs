using System.Numerics;
using Raylib_cs;

namespace Breakout;

sealed class CircleRendererComponent : Component
{
    public float Radius;
    public Vector2 Offset = Vector2.Zero;
    public Color FillColor;
    public Color? StrokeColor = null;
}
