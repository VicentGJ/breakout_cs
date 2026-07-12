using System.Numerics;


namespace Breakout;

static class BallObject
{
    public static GameObject Create(Scene scene, GameObject paddle, IReadOnlyList<GameObject> bricks)
    {
        GameObject gameObject = new("Ball");
        gameObject.ZIndex = 30;
        gameObject.Add(new BallComponent());
        gameObject.Add(new CircleRendererComponent { Radius = BreakoutRules.BallRadius, FillColor = BreakoutTheme.BallColor });
        gameObject.Add(new BoxColliderComponent { Size = new Vector2(BreakoutRules.BallRadius * 2f) });
        gameObject.Add(new AudioComponent
        {
            AssetPath = "Game/Assets/Audio/SFX/ball_bounce.wav"
        });
        gameObject.Add(new BallBehavior(paddle, bricks));

        scene.AddObject(gameObject);
        return gameObject;
    }
}
