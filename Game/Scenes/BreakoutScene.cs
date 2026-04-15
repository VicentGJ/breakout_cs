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

    public void Update()
    {
        if (Phase is GamePhase.Serving or GamePhase.Playing)
        {
            _paddleSystem.Update(this, InputState.Poll(), Time.DeltaTime);
        }

        switch (Phase)
        {
            case GamePhase.Serving:
                _ballSystem.StickToPaddle(this);

                if (InputState.Poll().LaunchPressed)
                {
                    _ballSystem.Launch(this);
                }

                break;

            case GamePhase.Playing:
                _ballSystem.Update(this, Time.DeltaTime);
                _collisionSystem.Resolve(this);
                _roundSystem.HandleBallOutOfBounds(this);
                break;

            case GamePhase.Won:
            case GamePhase.Lost:
                if (InputState.Poll().ConfirmPressed)
                {
                    _roundSystem.StartNewGame(this);
                }

                break;
        }
    }

    public void Draw()
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
            PaddleObject.Add(new CanvasItemComponent { Visible = true, ZIndex = 20, Modulate = Color.White });
            PaddleObject.Add(new PaddleComponent { Speed = BreakoutRules.PaddleSpeed });
            PaddleObject.Add(new RectangleRendererComponent { Size = BreakoutRules.PaddleSize, FillColor = BreakoutTheme.PaddleColor });
        }

        if (BallObject is null)
        {
            BallObject = CreateGameObject("Ball");
            BallObject.Add(new CanvasItemComponent { Visible = true, ZIndex = 30, Modulate = Color.White });
            BallObject.Add(new BallComponent());
            BallObject.Add(new CircleRendererComponent { Radius = BreakoutRules.BallRadius, FillColor = BreakoutTheme.BallColor });
        }

        PaddleObject.IsActive = true;
        BallObject.IsActive = true;

        Transform2D paddleTransform = PaddleObject.Transform2D;
        RectangleRendererComponent paddleRenderer = PaddleObject.Get<RectangleRendererComponent>();
        paddleTransform.Position = new Vector2(Arena.X + (Arena.Width - paddleRenderer.Size.X) * 0.5f, ArenaBottom - BreakoutRules.PaddleFloorOffset);
        paddleTransform.Rotation = 0f;
        paddleTransform.Scale = Vector2.One;
        PaddleObject.Transform2D = paddleTransform;

        Transform2D ballTransform = BallObject.Transform2D;
        BallComponent ball = BallObject.Get<BallComponent>();
        ballTransform.Position = Vector2.Zero;
        ballTransform.Rotation = 0f;
        ballTransform.Scale = Vector2.One;
        ball.PreviousPosition = Vector2.Zero;
        ball.Velocity = Vector2.Zero;
        BallObject.Transform2D = ballTransform;
        BallObject.Set(ball);
    }

    internal void CreateBrickObject(Vector2 position, Vector2 size, Color color, int points)
    {
        GameObject brick = CreateGameObject($"Brick-{Bricks.Count + 1}");
        brick.Add(new CanvasItemComponent { Visible = true, ZIndex = 10, Modulate = Color.White });
        brick.Add(new BrickComponent { Points = points });
        brick.Add(new RectangleRendererComponent
        {
            Size = size,
            FillColor = color,
            StrokeColor = BreakoutTheme.BackgroundColor
        });

        brick.Transform2D = new Transform2D { Position = position, Scale = Vector2.One };

        Bricks.Add(brick);
    }
}
