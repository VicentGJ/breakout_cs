using System.Numerics;
using Raylib_cs;

namespace Breakout;

static class BrickObject
{
    public static GameObject Create(Scene scene, int index, Vector2 position, Vector2 size, Color color, int points)
    {
        GameObject gameObject = new($"Brick-{index}")
        {
            Transform2D = new Transform2D { Position = position + size * 0.5f, Scale = Vector2.One },
            ZIndex = 10,
            Modulate = color
        };
        _ = gameObject.Add(new BrickComponent { Points = points });
        _ = gameObject.Add(new SpriteRendererComponent
        {
            AssetPath = "Game/Assets/Textures/brick.png",
            Size = size,
            Origin = size * 0.5f
        });
        _ = gameObject.Add(new BoxColliderComponent { Size = size });
        _ = gameObject.Add(new BrickBehavior());

        scene.AddObject(gameObject);
        return gameObject;
    }
}
