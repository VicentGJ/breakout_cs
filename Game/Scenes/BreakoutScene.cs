using System.Numerics;
using Raylib_cs;

namespace Breakout;

sealed class BreakoutScene : Scene
{
    private readonly BreakoutRenderer _renderer = new();

    public BreakoutScene(int screenWidth, int screenHeight)
    {
        ScreenWidth = screenWidth;
        ScreenHeight = screenHeight;
        Arena = new Rectangle(32f, BreakoutRules.HudHeight + 8f, screenWidth - 64f, screenHeight - BreakoutRules.HudHeight - 40f);

        GameObject gameState = CreateGameObject("GameState");
        gameState.AddBehavior(new RoundBehavior());
    }

    internal int ScreenWidth { get; }

    internal int ScreenHeight { get; }

    internal Rectangle Arena { get; }

    internal GamePhase Phase { get; set; }

    internal int Score { get; set; }

    internal int Lives { get; set; }

    internal int BricksRemaining { get; set; }

    internal List<GameObject> Bricks { get; } = [];

    internal InputState Input { get; private set; }

    internal GameObject PaddleObject { get; private set; } = null!;

    internal GameObject BallObject { get; private set; } = null!;

    internal float ArenaRight => Arena.X + Arena.Width;

    internal float ArenaBottom => Arena.Y + Arena.Height;

    public override void Update(float dt)
    {
        Input = InputState.Poll();
    }

    public override void Draw()
    {
        _renderer.Draw(this);
    }

    internal void ClearBricks()
    {
        foreach (GameObject brick in Bricks)
        {
            RemoveObject(brick);
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
            PaddleObject.Add(new RectangleRendererComponent { Size = BreakoutRules.PaddleSize, Origin = BreakoutRules.PaddleSize * 0.5f, FillColor = BreakoutTheme.PaddleColor });
            PaddleObject.Add(new BoxColliderComponent { Size = BreakoutRules.PaddleSize });
            PaddleObject.AddBehavior(new PaddleBehavior());
        }

        if (BallObject is null)
        {
            BallObject = CreateGameObject("Ball");
            BallObject.Add(new CanvasItemComponent { Visible = true, ZIndex = 30, Modulate = Color.White });
            BallObject.Add(new BallComponent());
            BallObject.Add(new CircleRendererComponent { Radius = BreakoutRules.BallRadius, FillColor = BreakoutTheme.BallColor });
            BallObject.Add(new BoxColliderComponent { Size = new Vector2(BreakoutRules.BallRadius * 2f) });
            BallObject.AddBehavior(new BallBehavior());
        }

        PaddleObject.Active = true;
        BallObject.Active = true;

        Transform2D paddleTransform = PaddleObject.Transform2D;
        paddleTransform.Position = new Vector2(Arena.X + Arena.Width * 0.5f, ArenaBottom - BreakoutRules.PaddleFloorOffset + BreakoutRules.PaddleSize.Y * 0.5f);
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
            Origin = size * 0.5f,
            FillColor = color,
            StrokeColor = BreakoutTheme.BackgroundColor
        });
        brick.Add(new BoxColliderComponent { Size = size });
        brick.AddBehavior(new BrickBehavior());

        brick.Transform2D = new Transform2D { Position = position + size * 0.5f, Scale = Vector2.One };

        Bricks.Add(brick);
    }
}
