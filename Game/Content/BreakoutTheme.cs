using Raylib_cs;

namespace Breakout;

internal static class BreakoutTheme
{
    public static readonly Color BackgroundColor = new(18, 22, 33, 255);
    public static readonly Color ArenaColor = new(25, 32, 48, 255);
    public static readonly Color BorderColor = new(90, 109, 143, 255);
    public static readonly Color TextColor = new(235, 239, 247, 255);
    public static readonly Color MutedTextColor = new(163, 177, 204, 255);
    public static readonly Color PaddleColor = new(242, 201, 76, 255);
    public static readonly Color BallColor = new(255, 255, 255, 255);
    public static readonly Color OverlayColor = new(8, 10, 18, 180);

    public static readonly Color[] BrickColors =
    [
        new(239, 71, 111, 255),
        new(255, 109, 89, 255),
        new(255, 209, 102, 255),
        new(6, 214, 160, 255),
        new(17, 138, 178, 255),
        new(99, 102, 241, 255)
    ];
}
