using System.Numerics;
using Raylib_cs;

namespace Breakout;

struct CircleRendererComponent : IComponent
{
    public float Radius;
    public Vector2 Offset = Vector2.Zero;
    public Color FillColor;
    public Color? StrokeColor = null;

    public CircleRendererComponent()
    {
    }
}
