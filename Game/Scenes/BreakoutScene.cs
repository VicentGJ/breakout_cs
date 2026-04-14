using System.Numerics;
using Raylib_cs;

namespace Breakout;

internal sealed class BreakoutScene : IGameScene
{
    private readonly BreakoutRenderer _renderer;
    private readonly PaddleSystem _paddleSystem;
    private readonly BallSystem _ballSystem;
    private readonly CollisionSystem _collisionSystem;
    private readonly RoundSystem _roundSystem;
    private int _nextGameObjectId = 1;

    public BreakoutScene(int screenWidth, int screenHeight)
    {
        ScreenWidth = screenWidth;
        ScreenHeight = screenHeight;
        Arena = new Rectangle(32f, BreakoutRules.HudHeight + 8f, screenWidth - 64f, screenHeight - BreakoutRules.HudHeight - 40f);
        _renderer = new BreakoutRenderer(this);
        _paddleSystem = new PaddleSystem();
        _ballSystem = new BallSystem();
        _collisionSystem = new CollisionSystem();
        _roundSystem = new RoundSystem(new LevelBuilder(), _ballSystem);

        _roundSystem.StartNewGame(this);
    }

    internal int ScreenWidth { get; }

    internal int ScreenHeight { get; }

    internal Rectangle Arena { get; }

    internal GamePhase Phase { get; set; }

    internal int Score { get; set; }

    internal int Lives { get; set; }

    internal int BricksRemaining { get; set; }

    internal List<GameObject> Objects { get; } = [];

    internal List<GameObject> Bricks { get; } = [];

    internal GameObject PaddleObject { get; private set; } = null!;

    internal GameObject BallObject { get; private set; } = null!;

    internal float ArenaRight => Arena.X + Arena.Width;

    internal float ArenaBottom => Arena.Y + Arena.Height;

    public void Update(FrameContext context)
    {
        if (Phase is GamePhase.Serving or GamePhase.Playing)
        {
            _paddleSystem.Update(this, context.Input, context.DeltaTime);
        }

        switch (Phase)
        {
            case GamePhase.Serving:
                _ballSystem.StickToPaddle(this);

                if (context.Input.LaunchPressed)
                {
                    _ballSystem.Launch(this);
                }

                break;

            case GamePhase.Playing:
                _ballSystem.Update(this, context.DeltaTime);
                _collisionSystem.Resolve(this);
                _roundSystem.HandleBallOutOfBounds(this);
                break;

            case GamePhase.Won:
            case GamePhase.Lost:
                if (context.Input.ConfirmPressed)
                {
                    _roundSystem.StartNewGame(this);
                }

                break;
        }
    }

    public void Draw(FrameContext context)
    {
        _renderer.Draw();
    }

    internal GameObject CreateGameObject(string name)
    {
        GameObject gameObject = new(_nextGameObjectId++, name);
        Objects.Add(gameObject);
        return gameObject;
    }

    internal void ClearBricks()
    {
        foreach (GameObject brick in Bricks)
        {
            Objects.Remove(brick);
        }

        Bricks.Clear();
    }

    internal void ResetCoreObjects()
    {
        if (PaddleObject is null)
        {
            PaddleObject = CreateGameObject("Paddle");
            PaddleObject.Add(Transform2DComponent.Default);
            PaddleObject.Add(new CanvasItemComponent { Visible = true, ZIndex = 20, Modulate = Color.White });
            PaddleObject.Add(new PaddleComponent { Speed = BreakoutRules.PaddleSpeed });
            PaddleObject.Add(new RectangleRendererComponent { Size = BreakoutRules.PaddleSize, FillColor = BreakoutTheme.PaddleColor });
        }

        if (BallObject is null)
        {
            BallObject = CreateGameObject("Ball");
            BallObject.Add(Transform2DComponent.Default);
            BallObject.Add(new CanvasItemComponent { Visible = true, ZIndex = 30, Modulate = Color.White });
            BallObject.Add(new BallComponent());
            BallObject.Add(new CircleRendererComponent { Radius = BreakoutRules.BallRadius, FillColor = BreakoutTheme.BallColor });
        }

        PaddleObject.IsActive = true;
        BallObject.IsActive = true;

        Transform2DComponent paddleTransform = PaddleObject.Get<Transform2DComponent>();
        RectangleRendererComponent paddleRenderer = PaddleObject.Get<RectangleRendererComponent>();
        paddleTransform.Position = new Vector2(Arena.X + (Arena.Width - paddleRenderer.Size.X) * 0.5f, ArenaBottom - BreakoutRules.PaddleFloorOffset);
        paddleTransform.Rotation = 0f;
        paddleTransform.Scale = Vector2.One;
        PaddleObject.Set(paddleTransform);

        Transform2DComponent ballTransform = BallObject.Get<Transform2DComponent>();
        BallComponent ball = BallObject.Get<BallComponent>();
        ballTransform.Position = Vector2.Zero;
        ballTransform.Rotation = 0f;
        ballTransform.Scale = Vector2.One;
        ball.PreviousPosition = Vector2.Zero;
        ball.Velocity = Vector2.Zero;
        BallObject.Set(ballTransform);
        BallObject.Set(ball);
    }

    internal void CreateBrickObject(Vector2 position, Vector2 size, Color color, int points)
    {
        GameObject brick = CreateGameObject($"Brick-{Bricks.Count + 1}");
        brick.Add(new Transform2DComponent { Position = position, Scale = Vector2.One });
        brick.Add(new CanvasItemComponent { Visible = true, ZIndex = 10, Modulate = Color.White });
        brick.Add(new BrickComponent { Points = points });
        brick.Add(new RectangleRendererComponent
        {
            Size = size,
            FillColor = color,
            StrokeColor = BreakoutTheme.BackgroundColor
        });

        Bricks.Add(brick);
    }
}
