using Raylib_cs;

namespace Breakout;

sealed class BreakoutRenderer
{
    private readonly RenderSystem _renderSystem;

    public BreakoutRenderer()
    {
        _renderSystem = new RenderSystem();
    }

    public void Draw(BreakoutScene scene)
    {
        Raylib.ClearBackground(BreakoutTheme.BackgroundColor);
        DrawArena(scene);
        _renderSystem.Draw(scene.Objects);
        DrawHud(scene);
        DrawOverlay(scene);
    }

    private static void DrawArena(BreakoutScene scene)
    {
        Raylib.DrawRectangle((int)scene.Arena.X, (int)scene.Arena.Y, (int)scene.Arena.Width, (int)scene.Arena.Height, BreakoutTheme.ArenaColor);
        Raylib.DrawRectangleLines((int)scene.Arena.X, (int)scene.Arena.Y, (int)scene.Arena.Width, (int)scene.Arena.Height, BreakoutTheme.BorderColor);
    }

    private static void DrawHud(BreakoutScene scene)
    {
        Raylib.DrawText($"Score: {scene.Score}", 32, 18, 24, BreakoutTheme.TextColor);

        string livesText = $"Lives: {scene.Lives}";
        int livesWidth = Raylib.MeasureText(livesText, 24);
        Raylib.DrawText(livesText, scene.ScreenWidth - livesWidth - 32, 18, 24, BreakoutTheme.TextColor);

        string controlsText = "Move: A/D or arrows   Launch: Space";
        int controlsWidth = Raylib.MeasureText(controlsText, 18);
        Raylib.DrawText(controlsText, (scene.ScreenWidth - controlsWidth) / 2, scene.ScreenHeight - 28, 18, BreakoutTheme.MutedTextColor);
    }

    private void DrawOverlay(BreakoutScene scene)
    {
        if (scene.Phase == GamePhase.Playing)
        {
            return;
        }

        string title;
        string subtitle;

        if (scene.Phase == GamePhase.Serving)
        {
            title = "Breakout";
            subtitle = "Press Space to launch the ball";
        }
        else if (scene.Phase == GamePhase.Won)
        {
            title = "You cleared the wall";
            subtitle = "Press Space or Enter to play again";
        }
        else
        {
            title = "Game over";
            subtitle = "Press Space or Enter to try again";
        }

        const int panelWidth = 420;
        const int panelHeight = 120;
        int panelX = (scene.ScreenWidth - panelWidth) / 2;
        int panelY = (scene.ScreenHeight - panelHeight) / 2;

        Raylib.DrawRectangle(panelX, panelY, panelWidth, panelHeight, BreakoutTheme.OverlayColor);
        Raylib.DrawRectangleLines(panelX, panelY, panelWidth, panelHeight, BreakoutTheme.BorderColor);

        DrawCenteredText(scene, title, panelY + 28, 32, BreakoutTheme.TextColor);
        DrawCenteredText(scene, subtitle, panelY + 72, 20, BreakoutTheme.MutedTextColor);
    }

    private static void DrawCenteredText(BreakoutScene scene, string text, int y, int fontSize, Color color)
    {
        int textWidth = Raylib.MeasureText(text, fontSize);
        Raylib.DrawText(text, (scene.ScreenWidth - textWidth) / 2, y, fontSize, color);
    }
}
