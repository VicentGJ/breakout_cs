using System.Numerics;
using Raylib_cs;

namespace Breakout;

sealed class BreakoutScene : Scene
{
    private readonly RenderSystem _renderSystem = new();

    public BreakoutScene(int screenWidth, int screenHeight)
    {
        ScreenWidth = screenWidth;
        ScreenHeight = screenHeight;
        Arena = new Rectangle(32f, BreakoutRules.HudHeight + 8f, screenWidth - 64f, screenHeight - BreakoutRules.HudHeight - 40f);

        GameObject gameState = CreateGameObject("GameState");
        gameState.AddBehavior(new RoundBehavior());

        CreatePresentationObjects();
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
        Raylib.ClearBackground(BreakoutTheme.BackgroundColor);
        _renderSystem.Draw(Objects);
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

    private void CreatePresentationObjects()
    {
        GameObject arenaObject = CreateRectangleObject(
            "Arena",
            new Vector2(Arena.X, Arena.Y),
            new Vector2(Arena.Width, Arena.Height),
            BreakoutTheme.ArenaColor,
            zIndex: 0);
        RectangleRendererComponent arenaRenderer = arenaObject.Get<RectangleRendererComponent>();
        arenaRenderer.StrokeColor = BreakoutTheme.BorderColor;
        arenaObject.Set(arenaRenderer);

        GameObject scoreText = CreateTextObject("ScoreText", new Vector2(32f, 18f), 24, BreakoutTheme.TextColor, zIndex: 100);
        GameObject livesText = CreateTextObject("LivesText", Vector2.Zero, 24, BreakoutTheme.TextColor, zIndex: 100);
        GameObject controlsText = CreateTextObject("ControlsText", Vector2.Zero, 18, BreakoutTheme.MutedTextColor, zIndex: 100);

        const int panelWidth = 420;
        const int panelHeight = 120;
        Vector2 panelPosition = new((ScreenWidth - panelWidth) * 0.5f, (ScreenHeight - panelHeight) * 0.5f);
        GameObject overlayPanel = CreateRectangleObject("OverlayPanel", panelPosition, new Vector2(panelWidth, panelHeight), BreakoutTheme.OverlayColor, zIndex: 200);
        RectangleRendererComponent overlayRenderer = overlayPanel.Get<RectangleRendererComponent>();
        overlayRenderer.StrokeColor = BreakoutTheme.BorderColor;
        overlayPanel.Set(overlayRenderer);

        GameObject overlayTitle = CreateTextObject("OverlayTitle", new Vector2(0f, panelPosition.Y + 28f), 32, BreakoutTheme.TextColor, zIndex: 201);
        GameObject overlaySubtitle = CreateTextObject("OverlaySubtitle", new Vector2(0f, panelPosition.Y + 72f), 20, BreakoutTheme.MutedTextColor, zIndex: 201);

        GameObject presentationState = CreateGameObject("PresentationState");
        presentationState.AddBehavior(new PresentationBehavior(scoreText, livesText, controlsText, overlayPanel, overlayTitle, overlaySubtitle));
    }

    private GameObject CreateRectangleObject(string name, Vector2 position, Vector2 size, Color fillColor, int zIndex)
    {
        GameObject gameObject = CreateGameObject(name);
        gameObject.Transform2D = new Transform2D { Position = position, Scale = Vector2.One };
        gameObject.Add(new CanvasItemComponent { Visible = true, ZIndex = zIndex, Modulate = Color.White });
        gameObject.Add(new RectangleRendererComponent { Size = size, FillColor = fillColor });
        return gameObject;
    }

    private GameObject CreateTextObject(string name, Vector2 position, int fontSize, Color color, int zIndex)
    {
        GameObject gameObject = CreateGameObject(name);
        gameObject.Transform2D = new Transform2D { Position = position, Scale = Vector2.One };
        gameObject.Add(new CanvasItemComponent { Visible = true, ZIndex = zIndex, Modulate = Color.White });
        gameObject.Add(new TextRendererComponent { FontSize = fontSize, Color = color });
        return gameObject;
    }
}
