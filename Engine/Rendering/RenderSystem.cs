using System.Numerics;
using Raylib_cs;

namespace Breakout;

sealed class RenderSystem
{
    private readonly IRenderComponentHandler[] _handlers =
    [
        new RectangleRenderHandler(),
        new CircleRenderHandler(),
        new TextRenderHandler()
    ];

    private readonly List<RenderEntry> _entries = [];

    public void Draw(IEnumerable<GameObject> objects)
    {
        _entries.Clear();

        foreach (GameObject gameObject in objects)
        {
            if (!gameObject.Active || !gameObject.Visible)
            {
                continue;
            }

            foreach (IRenderComponentHandler handler in _handlers)
            {
                if (!handler.CanDraw(gameObject))
                {
                    continue;
                }

                _entries.Add(new RenderEntry(gameObject, gameObject.Transform2D, handler));
                break;
            }
        }

        _entries.Sort(static (left, right) => left.GameObject.ZIndex.CompareTo(right.GameObject.ZIndex));

        foreach (RenderEntry entry in _entries)
        {
            entry.Handler.Draw(entry.GameObject, entry.Transform);
        }
    }

    private readonly record struct RenderEntry(GameObject GameObject, Transform2D Transform, IRenderComponentHandler Handler);
}

interface IRenderComponentHandler
{
    bool CanDraw(GameObject gameObject);

    void Draw(GameObject gameObject, in Transform2D transform);
}

abstract class RenderComponentHandler<T> : IRenderComponentHandler where T : struct, IComponent
{
    public bool CanDraw(GameObject gameObject)
    {
        return gameObject.Has<T>();
    }

    public void Draw(GameObject gameObject, in Transform2D transform)
    {
        Draw(gameObject, transform, gameObject.Get<T>());
    }

    protected abstract void Draw(GameObject gameObject, in Transform2D transform, in T renderer);

    protected static Color Modulate(Color color, Color modulate)
    {
        return new Color(
            (byte)(color.R * modulate.R / 255),
            (byte)(color.G * modulate.G / 255),
            (byte)(color.B * modulate.B / 255),
            (byte)(color.A * modulate.A / 255));
    }
}

sealed class RectangleRenderHandler : RenderComponentHandler<RectangleRendererComponent>
{
    protected override void Draw(GameObject gameObject, in Transform2D transform, in RectangleRendererComponent renderer)
    {
        Vector2 size = Vector2.Multiply(renderer.Size, transform.Scale);
        Rectangle rectangle = new(transform.Position.X + renderer.Offset.X, transform.Position.Y + renderer.Offset.Y, size.X, size.Y);
        Vector2 origin = Vector2.Multiply(renderer.Origin, transform.Scale);
        float rotationDegrees = transform.Rotation * (180f / MathF.PI);

        Raylib.DrawRectanglePro(rectangle, origin, rotationDegrees, Modulate(renderer.FillColor, gameObject.Modulate));

        if (renderer.StrokeColor is Color strokeColor && MathF.Abs(transform.Rotation) < 0.0001f)
        {
            Rectangle strokeRectangle = new(rectangle.X - origin.X, rectangle.Y - origin.Y, rectangle.Width, rectangle.Height);
            Raylib.DrawRectangleLinesEx(strokeRectangle, 1f, Modulate(strokeColor, gameObject.Modulate));
        }
    }
}

sealed class CircleRenderHandler : RenderComponentHandler<CircleRendererComponent>
{
    protected override void Draw(GameObject gameObject, in Transform2D transform, in CircleRendererComponent renderer)
    {
        Vector2 center = transform.Position + renderer.Offset;
        float scale = MathF.Max(MathF.Abs(transform.Scale.X), MathF.Abs(transform.Scale.Y));
        float radius = renderer.Radius * scale;
        Color color = Modulate(renderer.FillColor, gameObject.Modulate);

        Raylib.DrawCircleV(center, radius, color);

        if (renderer.StrokeColor is Color strokeColor)
        {
            Raylib.DrawCircleLinesV(center, radius, Modulate(strokeColor, gameObject.Modulate));
        }
    }
}

sealed class TextRenderHandler : RenderComponentHandler<TextRendererComponent>
{
    protected override void Draw(GameObject gameObject, in Transform2D transform, in TextRendererComponent renderer)
    {
        Vector2 position = transform.Position + renderer.Offset;
        Raylib.DrawText(renderer.Text, (int)position.X, (int)position.Y, renderer.FontSize, Modulate(renderer.Color, gameObject.Modulate));
    }
}
