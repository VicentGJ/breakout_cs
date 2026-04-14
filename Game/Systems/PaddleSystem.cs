namespace Breakout;

internal sealed class PaddleSystem
{
    public void Update(BreakoutScene scene, InputState input, float deltaTime)
    {
        Transform2DComponent transform = scene.PaddleObject.Get<Transform2DComponent>();
        PaddleComponent paddle = scene.PaddleObject.Get<PaddleComponent>();
        RectangleRendererComponent renderer = scene.PaddleObject.Get<RectangleRendererComponent>();

        transform.Position.X += input.HorizontalAxis * paddle.Speed * deltaTime;
        transform.Position.X = Math.Clamp(transform.Position.X, scene.Arena.X, scene.ArenaRight - renderer.Size.X);

        scene.PaddleObject.Set(transform);
    }
}
