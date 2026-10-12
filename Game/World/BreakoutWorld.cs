using System.Numerics;
using Raylib_cs;

namespace Breakout;

sealed class BreakoutWorld : World
{
    public BreakoutWorld(int screenWidth, int screenHeight)
    {
        ScreenWidth = screenWidth;
        ScreenHeight = screenHeight;
        Arena = new Rectangle(32f, BreakoutRules.HudHeight + 8f, screenWidth - 64f, screenHeight - BreakoutRules.HudHeight - 40f);
        BackgroundColor = BreakoutTheme.BackgroundColor;
    }

    internal int ScreenWidth { get; }

    internal int ScreenHeight { get; }

    internal Rectangle Arena { get; }

    internal InputState Input { get; private set; }

    internal float ArenaRight => Arena.X + Arena.Width;

    internal float ArenaBottom => Arena.Y + Arena.Height;

    public override void Setup()
    {
        AddObject(new GameState());
        LoadScene(CreateScene());
    }

    // Everything except GameState lives in the scene, so loading it again restarts the game.
    internal Scene CreateScene() => new("breakout", PopulateScene);

    private void PopulateScene()
    {
        AddObject(new Paddle());
        AddObject(new Ball());
        AddObject(new RectangleShape
        {
            Name = "Arena",
            Transform2D = new Transform2D { Position = new Vector2(Arena.X, Arena.Y), Scale = Vector2.One },
            Size = new Vector2(Arena.Width, Arena.Height),
            FillColor = BreakoutTheme.ArenaColor,
            StrokeColor = BreakoutTheme.BorderColor
        });
        AddObject(new Hud());

        foreach (Brick brick in BrickWallObjects.Create(Arena))
        {
            AddObject(brick);
        }
    }

    public override void Update(float dt)
    {
        Input = InputState.Poll();
    }

}
