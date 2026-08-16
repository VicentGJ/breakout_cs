using Raylib_cs;

namespace Breakout;

sealed class BreakoutScene : Scene
{
    public BreakoutScene(int screenWidth, int screenHeight)
    {
        ScreenWidth = screenWidth;
        ScreenHeight = screenHeight;
        Arena = new Rectangle(32f, BreakoutRules.HudHeight + 8f, screenWidth - 64f, screenHeight - BreakoutRules.HudHeight - 40f);
        BackgroundColor = BreakoutTheme.BackgroundColor;
    }

    internal int ScreenWidth { get; }

    internal int ScreenHeight { get; }

    internal Rectangle Arena { get; }

    internal GamePhase Phase { get; set; }

    internal int Score { get; set; }

    internal int Lives { get; set; }

    internal int BricksRemaining { get; set; }

    internal InputState Input { get; private set; }

    internal float ArenaRight => Arena.X + Arena.Width;

    internal float ArenaBottom => Arena.Y + Arena.Height;

    public override void Setup()
    {
        List<GameObject> bricks = [];
        GameObject gameState = GameStateObject.Create();
        GameObject paddle = PaddleObject.Create();
        GameObject ball = BallObject.Create(paddle, bricks);
        GameStateObject.AddRoundBehavior(gameState, paddle, ball, bricks);

        AddObject(gameState);
        AddObject(paddle);
        AddObject(ball);

        foreach (GameObject gameObject in PresentationObjects.Create(this))
        {
            AddObject(gameObject);
        }
    }

    public override void Update(float dt)
    {
        Input = InputState.Poll();
    }

}
