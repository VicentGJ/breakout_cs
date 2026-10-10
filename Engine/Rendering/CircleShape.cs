using System.Numerics;
using Raylib_cs;

namespace Breakout;

sealed class CircleShape : GameObject
{
    public float Radius;
    public Vector2 Offset = Vector2.Zero;
    public Color FillColor;
    public Color? StrokeColor = null;

    internal override void Draw(RenderSystem renderer, in Transform2D world)
    {
        Vector2 center = world.Position + Offset;
        float scale = MathF.Max(MathF.Abs(world.Scale.X), MathF.Abs(world.Scale.Y));
        float radius = Radius * scale;

        Raylib.DrawCircleV(center, radius, RenderSystem.Modulate(FillColor, Modulate));

        if (StrokeColor is Color strokeColor)
        {
            Raylib.DrawCircleLinesV(center, radius, RenderSystem.Modulate(strokeColor, Modulate));
        }
    }
}
