using System.Numerics;
using Raylib_cs;

namespace Breakout;

static class BrickWallObjects
{
    public static IReadOnlyList<GameObject> Create(Rectangle arena)
    {
        List<GameObject> bricks = [];
        const float sidePadding = 16f;
        float brickWidth = (arena.Width - sidePadding * 2f - BreakoutRules.BrickGap * (BreakoutRules.BrickColumns - 1)) / BreakoutRules.BrickColumns;
        float startX = arena.X + sidePadding;
        float startY = arena.Y + 28f;

        for (int row = 0; row < BreakoutRules.BrickRows; row++)
        {
            for (int column = 0; column < BreakoutRules.BrickColumns; column++)
            {
                float x = startX + column * (brickWidth + BreakoutRules.BrickGap);
                float y = startY + row * (BreakoutRules.BrickHeight + BreakoutRules.BrickGap);
                int points = (BreakoutRules.BrickRows - row) * 10;

                bricks.Add(BrickObject.Create(
                    bricks.Count + 1,
                    new Vector2(x, y),
                    new Vector2(brickWidth, BreakoutRules.BrickHeight),
                    BreakoutTheme.BrickColors[row],
                    points));
            }
        }

        return bricks;
    }
}
