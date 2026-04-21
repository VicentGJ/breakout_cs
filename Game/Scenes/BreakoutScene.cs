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
        GameObject gameState = GameStateObject.Create(this);
        List<GameObject> bricks = [];
        GameObject paddle = PaddleObject.Create(this);
        GameObject ball = BallObject.Create(this, paddle, bricks);
        GameStateObject.AddRoundBehavior(gameState, paddle, ball, bricks);
        PresentationObjects.Create(this);
    }

    public override void Update(float dt)
    {
        Input = InputState.Poll();
    }

    public override void Draw()
    {
        Raylib.ClearBackground(BreakoutTheme.BackgroundColor);
        _renderSystem.Draw(Objects);
    }
}
