using System.Numerics;
using Raylib_cs;

namespace Breakout;

sealed class Ball : GameObject
{
    private GameState _game = null!;
    private Paddle _paddle = null!;

    public Vector2 PreviousPosition;
    public Vector2 Velocity;

    public CircleShape Visual { get; }

    public BoxCollider Collider { get; }

    public AudioPlayer Bounce { get; }

    public Ball()
    {
        Visual = AddChild(new CircleShape
        {
            Name = "BallVisual",
            ZIndex = 30,
            Radius = BreakoutRules.BallRadius,
            FillColor = BreakoutTheme.BallColor
        });
        Collider = AddChild(new BoxCollider { Name = "BallCollider", Size = new Vector2(BreakoutRules.BallRadius * 2f) });
        Bounce = AddChild(new AudioPlayer { Name = "BallBounce", AssetPath = "Game/Assets/Audio/SFX/ball_bounce.wav" });
    }

    public override void Start()
    {
        _game = World.Find<GameState>()!;
        _paddle = World.Find<Paddle>()!;
    }

    public override void Update(float dt)
    {
        if (_game.Phase != GamePhase.Serving)
        {
            return;
        }

        StickToPaddle();

        if (((BreakoutWorld)World).Input.LaunchPressed)
        {
            Launch();
        }
    }

    public override void PhysicsUpdate(float dt)
    {
        if (_game.Phase != GamePhase.Playing)
        {
            return;
        }

        BreakoutWorld world = (BreakoutWorld)World;

        Move(dt);
        ResolveWallCollisions(world);
        ResolvePaddleCollision();
        ResolveBrickCollision();
        PublishOutOfBounds(world);
    }

    private void StickToPaddle()
    {
        Transform2D paddleTransform = _paddle.Transform2D;
        Transform2D ballTransform = Transform2D;

        ballTransform.Position = new Vector2(
            paddleTransform.Position.X,
            paddleTransform.Position.Y - _paddle.Collider.Size.Y * 0.5f - Visual.Radius - 2f);
        PreviousPosition = ballTransform.Position;
        Velocity = Vector2.Zero;

        Transform2D = ballTransform;
    }

    private void Launch()
    {
        float horizontalDirection = Raylib.GetRandomValue(0, 1) == 0 ? -0.65f : 0.65f;
        Velocity = Vector2.Normalize(new Vector2(horizontalDirection, -1f)) * BreakoutRules.BallSpeed;
        _game.Phase = GamePhase.Playing;
        World.Events.Publish(new BallLaunchedEvent(this));
    }

    private void Move(float dt)
    {
        Transform2D transform = Transform2D;

        PreviousPosition = transform.Position;
        transform.Position += Velocity * dt;

        Transform2D = transform;
    }

    private void ResolveWallCollisions(BreakoutWorld world)
    {
        Transform2D ballTransform = Transform2D;
        Vector2 ballHalfSize = Vector2.Abs(Collider.Size * ballTransform.Scale) * 0.5f;
        bool bounced = false;

        if (ballTransform.Position.X - ballHalfSize.X < world.Arena.X)
        {
            ballTransform.Position.X = world.Arena.X + ballHalfSize.X;
            Velocity.X = MathF.Abs(Velocity.X);
            bounced = true;
        }
        else if (ballTransform.Position.X + ballHalfSize.X > world.ArenaRight)
        {
            ballTransform.Position.X = world.ArenaRight - ballHalfSize.X;
            Velocity.X = -MathF.Abs(Velocity.X);
            bounced = true;
        }

        if (ballTransform.Position.Y - ballHalfSize.Y < world.Arena.Y)
        {
            ballTransform.Position.Y = world.Arena.Y + ballHalfSize.Y;
            Velocity.Y = MathF.Abs(Velocity.Y);
            bounced = true;
        }

        Transform2D = ballTransform;

        if (bounced)
        {
            Bounce.Play();
        }
    }

    private void ResolvePaddleCollision()
    {
        Transform2D ballTransform = Transform2D;
        Transform2D paddleTransform = _paddle.Transform2D;
        Collision collision = Collider.GetCollision(_paddle.Collider);

        if (Velocity.Y <= 0f || collision.Distance >= 0f)
        {
            return;
        }

        float hitOffset = (ballTransform.Position.X - paddleTransform.Position.X) / (_paddle.Collider.Size.X * 0.5f);
        float bounceX = Math.Clamp(hitOffset, -1f, 1f) * 0.9f;

        ballTransform.Position += CollisionHelper.GetCorrection(collision);
        ballTransform.Position.Y -= 0.5f;
        Velocity = Vector2.Normalize(new Vector2(bounceX, -1f)) * BreakoutRules.BallSpeed;

        Transform2D = ballTransform;
        Bounce.Play();
    }

    private void ResolveBrickCollision()
    {
        foreach (Brick brick in World.FindAll<Brick>())
        {
            if (!brick.ActiveInHierarchy)
            {
                continue;
            }

            Collision collision = Collider.GetCollision(brick.Collider);

            if (collision.Distance >= 0f)
            {
                continue;
            }

            brick.Destroy();
            BounceBall(collision);
            break;
        }
    }

    private void BounceBall(Collision collision)
    {
        Transform2D ballTransform = Transform2D;
        ballTransform.Position += CollisionHelper.GetCorrection(collision);
        ballTransform.Position -= collision.Normal * 0.5f;

        if (collision.Normal.X != 0f)
        {
            Velocity.X = collision.Normal.X < 0f ? MathF.Abs(Velocity.X) : -MathF.Abs(Velocity.X);
        }
        else
        {
            Velocity.Y = collision.Normal.Y < 0f ? MathF.Abs(Velocity.Y) : -MathF.Abs(Velocity.Y);
        }

        Transform2D = ballTransform;
        Bounce.Play();
    }

    private void PublishOutOfBounds(BreakoutWorld world)
    {
        Transform2D ballTransform = Transform2D;
        float ballHalfHeight = MathF.Abs(Collider.Size.Y * ballTransform.Scale.Y) * 0.5f;

        if (ballTransform.Position.Y - ballHalfHeight > world.ArenaBottom)
        {
            World.Events.Publish(new BallOutOfBoundsEvent(this));
        }
    }
}
