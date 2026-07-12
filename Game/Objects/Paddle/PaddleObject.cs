

namespace Breakout;

static class PaddleObject
{
    public static GameObject Create(Scene scene)
    {
        GameObject gameObject = new("Paddle");
        gameObject.ZIndex = 20;
        gameObject.Add(new PaddleComponent { Speed = BreakoutRules.PaddleSpeed });
        gameObject.Add(new RectangleRendererComponent { Size = BreakoutRules.PaddleSize, Origin = BreakoutRules.PaddleSize * 0.5f, FillColor = BreakoutTheme.PaddleColor });
        gameObject.Add(new BoxColliderComponent { Size = BreakoutRules.PaddleSize });
        gameObject.AddBehavior(new PaddleBehavior());

        scene.AddObject(gameObject);
        return gameObject;
    }
}
