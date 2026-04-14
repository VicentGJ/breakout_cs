using System.Numerics;
using Raylib_cs;

namespace Breakout;

internal static class GameObjectGeometry
{
    public static Rectangle GetRectangle(GameObject gameObject)
    {
        Transform2DComponent transform = gameObject.Get<Transform2DComponent>();
        RectangleRendererComponent renderer = gameObject.Get<RectangleRendererComponent>();
        Vector2 size = Vector2.Multiply(renderer.Size, transform.Scale);
        Vector2 position = transform.Position + renderer.Offset;
        return new Rectangle(position.X, position.Y, size.X, size.Y);
    }

    public static Vector2 GetCircleCenter(GameObject gameObject)
    {
        Transform2DComponent transform = gameObject.Get<Transform2DComponent>();
        CircleRendererComponent renderer = gameObject.Get<CircleRendererComponent>();
        return transform.Position + renderer.Offset;
    }

    public static float GetCircleRadius(GameObject gameObject)
    {
        Transform2DComponent transform = gameObject.Get<Transform2DComponent>();
        CircleRendererComponent renderer = gameObject.Get<CircleRendererComponent>();
        float scale = MathF.Max(MathF.Abs(transform.Scale.X), MathF.Abs(transform.Scale.Y));
        return renderer.Radius * scale;
    }
}
