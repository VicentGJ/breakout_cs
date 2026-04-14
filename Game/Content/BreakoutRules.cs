using System.Numerics;

namespace Breakout;

internal static class BreakoutRules
{
    public const int BrickColumns = 10;
    public const int BrickRows = 6;
    public const float BrickHeight = 24f;
    public const float BrickGap = 8f;
    public const float BallRadius = 9f;
    public const float PaddleSpeed = 560f;
    public const float BallSpeed = 420f;
    public const int StartingLives = 3;
    public const int HudHeight = 56;
    public const float PaddleFloorOffset = 40f;

    public static readonly Vector2 PaddleSize = new(112f, 18f);
}
