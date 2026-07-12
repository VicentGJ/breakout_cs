using System.Numerics;
using Raylib_cs;

namespace Breakout;

static class BrickObject
{
    public static GameObject Create(Scene scene, int index, Vector2 position, Vector2 size, Color color, int points)
    {
        GameObject gameObject = new($"Brick-{index}");
        gameObject.Transform2D = new Transform2D { Position = position + size * 0.5f, Scale = Vector2.One };
        gameObject.ZIndex = 10;
        gameObject.Add(new BrickComponent { Points = points });
        gameObject.Add(new RectangleRendererComponent
        {
            Size = size,
            Origin = size * 0.5f,
            FillColor = color,
            StrokeColor = BreakoutTheme.BackgroundColor
        });
        gameObject.Add(new BoxColliderComponent { Size = size });
        gameObject.AddBehavior(new BrickBehavior());

        scene.AddObject(gameObject);
        return gameObject;
    }
}
