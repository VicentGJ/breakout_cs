namespace Breakout;

sealed class PaddleSystem
{
    public void Update(BreakoutScene scene, InputState input, float deltaTime)
    {
        Transform2D transform = scene.PaddleObject.Transform2D;
        PaddleComponent paddle = scene.PaddleObject.Get<PaddleComponent>();
        RectangleRendererComponent renderer = scene.PaddleObject.Get<RectangleRendererComponent>();
        float halfWidth = MathF.Abs(renderer.Size.X * transform.Scale.X) * 0.5f;

        transform.Position.X += input.HorizontalAxis * paddle.Speed * deltaTime;
        transform.Position.X = Math.Clamp(transform.Position.X, scene.Arena.X + halfWidth, scene.ArenaRight - halfWidth);

        scene.PaddleObject.Transform2D = transform;
    }
}
