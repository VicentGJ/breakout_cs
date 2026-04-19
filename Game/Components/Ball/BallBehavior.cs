using System.Numerics;
using Raylib_cs;

namespace Breakout;

sealed class BallBehavior : BehaviorComponent
{
    public override void Update(float dt)
    {
        BreakoutScene scene = (BreakoutScene)Scene;

        if (scene.Phase != GamePhase.Serving)
        {
            return;
        }

        StickToPaddle(scene);

        if (scene.Input.LaunchPressed)
        {
            Launch(scene);
        }
    }

    public override void PhysicsUpdate(float dt)
    {
        BreakoutScene scene = (BreakoutScene)Scene;

        if (scene.Phase != GamePhase.Playing)
        {
            return;
        }

        Move(dt);
        ResolveWallCollisions(scene);
        ResolvePaddleCollision(scene);
        ResolveBrickCollision(scene);
        PublishOutOfBounds(scene);
    }

    private void StickToPaddle(BreakoutScene scene)
    {
        Transform2D paddleTransform = scene.PaddleObject.Transform2D;
        RectangleRendererComponent paddleRenderer = scene.PaddleObject.Get<RectangleRendererComponent>();
        Transform2D ballTransform = Owner.Transform2D;
        BallComponent ball = Owner.Get<BallComponent>();
        CircleRendererComponent ballRenderer = Owner.Get<CircleRendererComponent>();

        ballTransform.Position = new Vector2(
            paddleTransform.Position.X,
            paddleTransform.Position.Y - paddleRenderer.Size.Y * 0.5f - ballRenderer.Radius - 2f);
        ball.PreviousPosition = ballTransform.Position;
        ball.Velocity = Vector2.Zero;

        Owner.Transform2D = ballTransform;
        Owner.Set(ball);
    }

    private void Launch(BreakoutScene scene)
    {
        float horizontalDirection = Raylib.GetRandomValue(0, 1) == 0 ? -0.65f : 0.65f;
        BallComponent ball = Owner.Get<BallComponent>();
        ball.Velocity = Vector2.Normalize(new Vector2(horizontalDirection, -1f)) * BreakoutRules.BallSpeed;
        Owner.Set(ball);
        scene.Phase = GamePhase.Playing;
        Scene.Events.Publish(new BallLaunchedEvent(Owner));
    }

    private void Move(float dt)
    {
        Transform2D transform = Owner.Transform2D;
        BallComponent ball = Owner.Get<BallComponent>();

        ball.PreviousPosition = transform.Position;
        transform.Position += ball.Velocity * dt;

        Owner.Transform2D = transform;
        Owner.Set(ball);
    }

    private void ResolveWallCollisions(BreakoutScene scene)
    {
        Transform2D ballTransform = Owner.Transform2D;
        BallComponent ball = Owner.Get<BallComponent>();
        BoxColliderComponent ballCollider = Owner.Get<BoxColliderComponent>();
        Vector2 ballHalfSize = Vector2.Abs(ballCollider.Size * ballTransform.Scale) * 0.5f;

        if (ballTransform.Position.X - ballHalfSize.X < scene.Arena.X)
        {
            ballTransform.Position.X = scene.Arena.X + ballHalfSize.X;
            ball.Velocity.X = MathF.Abs(ball.Velocity.X);
        }
        else if (ballTransform.Position.X + ballHalfSize.X > scene.ArenaRight)
        {
            ballTransform.Position.X = scene.ArenaRight - ballHalfSize.X;
            ball.Velocity.X = -MathF.Abs(ball.Velocity.X);
        }

        if (ballTransform.Position.Y - ballHalfSize.Y < scene.Arena.Y)
        {
            ballTransform.Position.Y = scene.Arena.Y + ballHalfSize.Y;
            ball.Velocity.Y = MathF.Abs(ball.Velocity.Y);
        }

        Owner.Transform2D = ballTransform;
        Owner.Set(ball);
    }

    private void ResolvePaddleCollision(BreakoutScene scene)
    {
        Transform2D ballTransform = Owner.Transform2D;
        BallComponent ball = Owner.Get<BallComponent>();
        BoxColliderComponent ballCollider = Owner.Get<BoxColliderComponent>();
        BoxColliderComponent paddleCollider = scene.PaddleObject.Get<BoxColliderComponent>();
        Transform2D paddleTransform = scene.PaddleObject.Transform2D;
        Collision collision = ballCollider.GetCollision(paddleCollider, ballTransform, paddleTransform);

        if (ball.Velocity.Y <= 0f || collision.Distance >= 0f)
        {
            return;
        }

        float hitOffset = (ballTransform.Position.X - paddleTransform.Position.X) / (paddleCollider.Size.X * 0.5f);
        float bounceX = Math.Clamp(hitOffset, -1f, 1f) * 0.9f;

        ballTransform.Position += CollisionHelper.GetCorrection(collision);
        ballTransform.Position.Y -= 0.5f;
        ball.Velocity = Vector2.Normalize(new Vector2(bounceX, -1f)) * BreakoutRules.BallSpeed;

        Owner.Transform2D = ballTransform;
        Owner.Set(ball);
    }

    private void ResolveBrickCollision(BreakoutScene scene)
    {
        Transform2D ballTransform = Owner.Transform2D;
        BoxColliderComponent ballCollider = Owner.Get<BoxColliderComponent>();

        foreach (GameObject brickObject in scene.Bricks)
        {
            if (!brickObject.Active)
            {
                continue;
            }

            BoxColliderComponent brickCollider = brickObject.Get<BoxColliderComponent>();
            Collision collision = ballCollider.GetCollision(brickCollider, ballTransform, brickObject.Transform2D);

            if (collision.Distance >= 0f)
            {
                continue;
            }

            brickObject.GetBehavior<BrickBehavior>().DestroyBrick();
            BounceBall(collision);
            break;
        }
    }

    private void BounceBall(Collision collision)
    {
        Transform2D ballTransform = Owner.Transform2D;
        BallComponent ball = Owner.Get<BallComponent>();
        ballTransform.Position += CollisionHelper.GetCorrection(collision);
        ballTransform.Position -= collision.Normal * 0.5f;

        if (collision.Normal.X != 0f)
        {
            ball.Velocity.X = collision.Normal.X < 0f ? MathF.Abs(ball.Velocity.X) : -MathF.Abs(ball.Velocity.X);
        }
        else
        {
            ball.Velocity.Y = collision.Normal.Y < 0f ? MathF.Abs(ball.Velocity.Y) : -MathF.Abs(ball.Velocity.Y);
        }

        Owner.Transform2D = ballTransform;
        Owner.Set(ball);
    }

    private void PublishOutOfBounds(BreakoutScene scene)
    {
        Transform2D ballTransform = Owner.Transform2D;
        BoxColliderComponent ballCollider = Owner.Get<BoxColliderComponent>();
        float ballHalfHeight = MathF.Abs(ballCollider.Size.Y * ballTransform.Scale.Y) * 0.5f;

        if (ballTransform.Position.Y - ballHalfHeight > scene.ArenaBottom)
        {
            Scene.Events.Publish(new BallOutOfBoundsEvent(Owner));
        }
    }
}
