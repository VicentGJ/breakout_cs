using System.Numerics;
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

    internal InputState Input { get; private set; }

    internal float ArenaRight => Arena.X + Arena.Width;

    internal float ArenaBottom => Arena.Y + Arena.Height;

    public override void Setup()
    {
        AddObject(new GameState());
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
    }

    public override void Update(float dt)
    {
        Input = InputState.Poll();
    }

}
