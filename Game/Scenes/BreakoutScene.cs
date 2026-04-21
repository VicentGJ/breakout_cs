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

        PresentationObjects.Create(this);
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
            PaddleObject = global::Breakout.PaddleObject.Create(this);
        }

        if (BallObject is null)
        {
            BallObject = global::Breakout.BallObject.Create(this);
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
        GameObject brick = BrickObject.Create(this, Bricks.Count + 1, position, size, color, points);
        Bricks.Add(brick);
    }
}
