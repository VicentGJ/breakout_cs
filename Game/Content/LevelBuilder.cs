using System.Numerics;

namespace Breakout;

sealed class LevelBuilder
{
    public void Build(BreakoutScene scene)
    {
        scene.ClearBricks();

        const float sidePadding = 16f;
        float brickWidth = (scene.Arena.Width - sidePadding * 2f - BreakoutRules.BrickGap * (BreakoutRules.BrickColumns - 1)) / BreakoutRules.BrickColumns;
        float startX = scene.Arena.X + sidePadding;
        float startY = scene.Arena.Y + 28f;

        for (int row = 0; row < BreakoutRules.BrickRows; row++)
        {
            for (int column = 0; column < BreakoutRules.BrickColumns; column++)
            {
                float x = startX + column * (brickWidth + BreakoutRules.BrickGap);
                float y = startY + row * (BreakoutRules.BrickHeight + BreakoutRules.BrickGap);
                int points = (BreakoutRules.BrickRows - row) * 10;

                scene.CreateBrickObject(
                    new Vector2(x, y),
                    new Vector2(brickWidth, BreakoutRules.BrickHeight),
                    BreakoutTheme.BrickColors[row],
                    points);
            }
        }

        scene.BricksRemaining = scene.Bricks.Count;
    }
}
