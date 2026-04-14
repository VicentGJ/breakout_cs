using System.Numerics;
using Raylib_cs;

namespace Breakout;

internal sealed class BallSystem
{
    public void StickToPaddle(BreakoutScene scene)
    {
        Transform2DComponent paddleTransform = scene.PaddleObject.Get<Transform2DComponent>();
        RectangleRendererComponent paddleRenderer = scene.PaddleObject.Get<RectangleRendererComponent>();
        Transform2DComponent ballTransform = scene.BallObject.Get<Transform2DComponent>();
        BallComponent ball = scene.BallObject.Get<BallComponent>();
        CircleRendererComponent ballRenderer = scene.BallObject.Get<CircleRendererComponent>();

        ballTransform.Position = new Vector2(
            paddleTransform.Position.X + paddleRenderer.Size.X * 0.5f,
            paddleTransform.Position.Y - ballRenderer.Radius - 2f);
        ball.PreviousPosition = ballTransform.Position;
        ball.Velocity = Vector2.Zero;

        scene.BallObject.Set(ballTransform);
        scene.BallObject.Set(ball);
    }

    public void Launch(BreakoutScene scene)
    {
        float horizontalDirection = Raylib.GetRandomValue(0, 1) == 0 ? -0.65f : 0.65f;
        BallComponent ball = scene.BallObject.Get<BallComponent>();
        ball.Velocity = Vector2.Normalize(new Vector2(horizontalDirection, -1f)) * BreakoutRules.BallSpeed;
        scene.BallObject.Set(ball);
        scene.Phase = GamePhase.Playing;
    }

    public void Update(BreakoutScene scene, float deltaTime)
    {
        Transform2DComponent transform = scene.BallObject.Get<Transform2DComponent>();
        BallComponent ball = scene.BallObject.Get<BallComponent>();

        ball.PreviousPosition = transform.Position;
        transform.Position += ball.Velocity * deltaTime;

        scene.BallObject.Set(transform);
        scene.BallObject.Set(ball);
    }
}
