using System.Numerics;

namespace Breakout;

sealed class CollisionSystem
{
    public void Resolve(BreakoutScene scene)
    {
        ResolveWallCollisions(scene);
        ResolvePaddleCollision(scene);
        ResolveBrickCollision(scene);
    }

    private static void ResolveWallCollisions(BreakoutScene scene)
    {
        Transform2D ballTransform = scene.BallObject.Transform2D;
        BallComponent ball = scene.BallObject.Get<BallComponent>();
        BoxColliderComponent ballCollider = scene.BallObject.Get<BoxColliderComponent>();
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

        scene.BallObject.Transform2D = ballTransform;
        scene.BallObject.Set(ball);
    }

    private static void ResolvePaddleCollision(BreakoutScene scene)
    {
        Transform2D ballTransform = scene.BallObject.Transform2D;
        BallComponent ball = scene.BallObject.Get<BallComponent>();
        BoxColliderComponent ballCollider = scene.BallObject.Get<BoxColliderComponent>();
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

        scene.BallObject.Transform2D = ballTransform;
        scene.BallObject.Set(ball);
    }

    private static void ResolveBrickCollision(BreakoutScene scene)
    {
        Transform2D ballTransform = scene.BallObject.Transform2D;
        BallComponent ball = scene.BallObject.Get<BallComponent>();
        BoxColliderComponent ballCollider = scene.BallObject.Get<BoxColliderComponent>();

        foreach (GameObject brickObject in scene.Bricks)
        {
            if (!brickObject.IsActive)
            {
                continue;
            }

            BoxColliderComponent brickCollider = brickObject.Get<BoxColliderComponent>();
            Collision collision = ballCollider.GetCollision(brickCollider, ballTransform, brickObject.Transform2D);

            if (collision.Distance >= 0f)
            {
                continue;
            }

            BrickComponent brick = brickObject.Get<BrickComponent>();
            CanvasItemComponent canvasItem = brickObject.Get<CanvasItemComponent>();

            brickObject.IsActive = false;
            canvasItem.Visible = false;
            brickObject.Set(canvasItem);
            scene.BricksRemaining--;
            scene.Score += brick.Points;

            BounceBall(scene, collision);

            if (scene.BricksRemaining == 0)
            {
                scene.Phase = GamePhase.Won;
            }

            break;
        }
    }

    private static void BounceBall(BreakoutScene scene, Collision collision)
    {
        Transform2D ballTransform = scene.BallObject.Transform2D;
        BallComponent ball = scene.BallObject.Get<BallComponent>();
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

        scene.BallObject.Transform2D = ballTransform;
        scene.BallObject.Set(ball);
    }
}
