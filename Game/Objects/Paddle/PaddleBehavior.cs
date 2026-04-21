namespace Breakout;

sealed class PaddleBehavior : BehaviorComponent
{
    public override void Update(float dt)
    {
        BreakoutScene scene = (BreakoutScene)Scene;

        if (scene.Phase is not (GamePhase.Serving or GamePhase.Playing))
        {
            return;
        }

        Transform2D transform = Owner.Transform2D;
        PaddleComponent paddle = Owner.Get<PaddleComponent>();
        RectangleRendererComponent renderer = Owner.Get<RectangleRendererComponent>();
        float halfWidth = MathF.Abs(renderer.Size.X * transform.Scale.X) * 0.5f;

        transform.Position.X += scene.Input.HorizontalAxis * paddle.Speed * dt;
        transform.Position.X = Math.Clamp(transform.Position.X, scene.Arena.X + halfWidth, scene.ArenaRight - halfWidth);

        Owner.Transform2D = transform;
    }
}
