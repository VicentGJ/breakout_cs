using System.Numerics;
using Raylib_cs;

namespace Breakout;

internal sealed class CollisionSystem
{
    public void Resolve(BreakoutScene scene)
    {
        ResolveWallCollisions(scene);
        ResolvePaddleCollision(scene);
        ResolveBrickCollision(scene);
    }

    private static void ResolveWallCollisions(BreakoutScene scene)
    {
        Transform2DComponent ballTransform = scene.BallObject.Get<Transform2DComponent>();
        BallComponent ball = scene.BallObject.Get<BallComponent>();
        float ballRadius = GameObjectGeometry.GetCircleRadius(scene.BallObject);

        if (ballTransform.Position.X - ballRadius < scene.Arena.X)
        {
            ballTransform.Position.X = scene.Arena.X + ballRadius;
            ball.Velocity.X = MathF.Abs(ball.Velocity.X);
        }
        else if (ballTransform.Position.X + ballRadius > scene.ArenaRight)
        {
            ballTransform.Position.X = scene.ArenaRight - ballRadius;
            ball.Velocity.X = -MathF.Abs(ball.Velocity.X);
        }

        if (ballTransform.Position.Y - ballRadius < scene.Arena.Y)
        {
            ballTransform.Position.Y = scene.Arena.Y + ballRadius;
            ball.Velocity.Y = MathF.Abs(ball.Velocity.Y);
        }

        scene.BallObject.Set(ballTransform);
        scene.BallObject.Set(ball);
    }

    private static void ResolvePaddleCollision(BreakoutScene scene)
    {
        Rectangle paddleBounds = GameObjectGeometry.GetRectangle(scene.PaddleObject);
        Transform2DComponent ballTransform = scene.BallObject.Get<Transform2DComponent>();
        BallComponent ball = scene.BallObject.Get<BallComponent>();
        float ballRadius = GameObjectGeometry.GetCircleRadius(scene.BallObject);
        float paddleCenterX = paddleBounds.X + paddleBounds.Width * 0.5f;

        if (ball.Velocity.Y <= 0f || !Raylib.CheckCollisionCircleRec(ballTransform.Position, ballRadius, paddleBounds))
        {
            return;
        }

        float hitOffset = (ballTransform.Position.X - paddleCenterX) / (paddleBounds.Width * 0.5f);
        float bounceX = Math.Clamp(hitOffset, -1f, 1f) * 0.9f;

        ballTransform.Position.Y = paddleBounds.Y - ballRadius - 0.5f;
        ball.Velocity = Vector2.Normalize(new Vector2(bounceX, -1f)) * BreakoutRules.BallSpeed;

        scene.BallObject.Set(ballTransform);
        scene.BallObject.Set(ball);
    }

    private static void ResolveBrickCollision(BreakoutScene scene)
    {
        Transform2DComponent ballTransform = scene.BallObject.Get<Transform2DComponent>();
        BallComponent ball = scene.BallObject.Get<BallComponent>();
        float ballRadius = GameObjectGeometry.GetCircleRadius(scene.BallObject);

        foreach (GameObject brickObject in scene.Bricks)
        {
            if (!brickObject.IsActive)
            {
                continue;
            }

            Rectangle brickBounds = GameObjectGeometry.GetRectangle(brickObject);

            if (!Raylib.CheckCollisionCircleRec(ballTransform.Position, ballRadius, brickBounds))
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

            BounceBallFromRectangle(scene, brickBounds);

            if (scene.BricksRemaining == 0)
            {
                scene.Phase = GamePhase.Won;
            }

            break;
        }
    }

    private static void BounceBallFromRectangle(BreakoutScene scene, Rectangle rectangle)
    {
        Transform2DComponent ballTransform = scene.BallObject.Get<Transform2DComponent>();
        BallComponent ball = scene.BallObject.Get<BallComponent>();
        float ballRadius = GameObjectGeometry.GetCircleRadius(scene.BallObject);

        if (ball.PreviousPosition.Y + ballRadius <= rectangle.Y)
        {
            ballTransform.Position.Y = rectangle.Y - ballRadius - 0.5f;
            ball.Velocity.Y = -MathF.Abs(ball.Velocity.Y);
            scene.BallObject.Set(ballTransform);
            scene.BallObject.Set(ball);
            return;
        }

        if (ball.PreviousPosition.Y - ballRadius >= RectangleBottom(rectangle))
        {
            ballTransform.Position.Y = RectangleBottom(rectangle) + ballRadius + 0.5f;
            ball.Velocity.Y = MathF.Abs(ball.Velocity.Y);
            scene.BallObject.Set(ballTransform);
            scene.BallObject.Set(ball);
            return;
        }

        if (ball.PreviousPosition.X + ballRadius <= rectangle.X)
        {
            ballTransform.Position.X = rectangle.X - ballRadius - 0.5f;
            ball.Velocity.X = -MathF.Abs(ball.Velocity.X);
            scene.BallObject.Set(ballTransform);
            scene.BallObject.Set(ball);
            return;
        }

        ballTransform.Position.X = RectangleRight(rectangle) + ballRadius + 0.5f;
        ball.Velocity.X = MathF.Abs(ball.Velocity.X);
        scene.BallObject.Set(ballTransform);
        scene.BallObject.Set(ball);
    }

    private static float RectangleRight(Rectangle rectangle) => rectangle.X + rectangle.Width;

    private static float RectangleBottom(Rectangle rectangle) => rectangle.Y + rectangle.Height;
}
