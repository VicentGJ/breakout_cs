using System.Numerics;
using Raylib_cs;

namespace Breakout;

static class BallObject
{
    public static GameObject Create(Scene scene, GameObject paddle, IReadOnlyList<GameObject> bricks)
    {
        GameObject gameObject = new("Ball");
        gameObject.Add(new CanvasItemComponent { Visible = true, ZIndex = 30, Modulate = Color.White });
        gameObject.Add(new BallComponent());
        gameObject.Add(new CircleRendererComponent { Radius = BreakoutRules.BallRadius, FillColor = BreakoutTheme.BallColor });
        gameObject.Add(new BoxColliderComponent { Size = new Vector2(BreakoutRules.BallRadius * 2f) });
        gameObject.AddBehavior(new BallBehavior(paddle, bricks));

        scene.AddObject(gameObject);
        return gameObject;
    }
}
