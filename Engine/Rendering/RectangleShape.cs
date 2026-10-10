using System.Numerics;
using Raylib_cs;

namespace Breakout;

sealed class RectangleShape : GameObject
{
    public Vector2 Size;
    public Vector2 Offset = Vector2.Zero;
    public Vector2 Origin = Vector2.Zero;
    public Color FillColor;
    public Color? StrokeColor = null;

    internal override void Draw(RenderSystem renderer, in Transform2D world)
    {
        Vector2 size = Vector2.Multiply(Size, world.Scale);
        Rectangle rectangle = new(world.Position.X + Offset.X, world.Position.Y + Offset.Y, size.X, size.Y);
        Vector2 origin = Vector2.Multiply(Origin, world.Scale);
        float rotationDegrees = world.Rotation * (180f / MathF.PI);

        Raylib.DrawRectanglePro(rectangle, origin, rotationDegrees, RenderSystem.Modulate(FillColor, Modulate));

        if (StrokeColor is Color strokeColor && MathF.Abs(world.Rotation) < 0.0001f)
        {
            Rectangle strokeRectangle = new(rectangle.X - origin.X, rectangle.Y - origin.Y, rectangle.Width, rectangle.Height);
            Raylib.DrawRectangleLinesEx(strokeRectangle, 1f, RenderSystem.Modulate(strokeColor, Modulate));
        }
    }
}
