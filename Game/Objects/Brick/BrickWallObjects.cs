using System.Numerics;
using Raylib_cs;

namespace Breakout;

static class BrickWallObjects
{
    public static IReadOnlyList<Brick> Create(Rectangle arena)
    {
        List<Brick> bricks = [];
        const float sidePadding = 16f;
        float brickWidth = (arena.Width - sidePadding * 2f - BreakoutRules.BrickGap * (BreakoutRules.BrickColumns - 1)) / BreakoutRules.BrickColumns;
        Vector2 size = new(brickWidth, BreakoutRules.BrickHeight);
        float startX = arena.X + sidePadding;
        float startY = arena.Y + 28f;

        for (int row = 0; row < BreakoutRules.BrickRows; row++)
        {
            for (int column = 0; column < BreakoutRules.BrickColumns; column++)
            {
                float x = startX + column * (brickWidth + BreakoutRules.BrickGap);
                float y = startY + row * (BreakoutRules.BrickHeight + BreakoutRules.BrickGap);

                Brick brick = new()
                {
                    Name = $"Brick-{bricks.Count + 1}",
                    Transform2D = new Transform2D { Position = new Vector2(x, y) + size * 0.5f, Scale = Vector2.One },
                    Points = (BreakoutRules.BrickRows - row) * 10
                };
                brick.Sprite.Size = size;
                brick.Sprite.Origin = size * 0.5f;
                brick.Sprite.Modulate = BreakoutTheme.BrickColors[row];
                brick.Collider.Size = size;

                bricks.Add(brick);
            }
        }

        return bricks;
    }
}
