using Raylib_cs;

namespace Breakout;

internal sealed class BreakoutRenderer
{
    private readonly BreakoutScene _scene;
    private readonly RenderSystem _renderSystem;

    public BreakoutRenderer(BreakoutScene scene)
    {
        _scene = scene;
        _renderSystem = new RenderSystem();
    }

    public void Draw()
    {
        Raylib.ClearBackground(BreakoutTheme.BackgroundColor);
        DrawArena();
        _renderSystem.Draw(_scene.Objects);
        DrawHud();
        DrawOverlay();
    }

    private void DrawArena()
    {
        Raylib.DrawRectangle((int)_scene.Arena.X, (int)_scene.Arena.Y, (int)_scene.Arena.Width, (int)_scene.Arena.Height, BreakoutTheme.ArenaColor);
        Raylib.DrawRectangleLines((int)_scene.Arena.X, (int)_scene.Arena.Y, (int)_scene.Arena.Width, (int)_scene.Arena.Height, BreakoutTheme.BorderColor);
    }

    private void DrawHud()
    {
        Raylib.DrawText($"Score: {_scene.Score}", 32, 18, 24, BreakoutTheme.TextColor);

        string livesText = $"Lives: {_scene.Lives}";
        int livesWidth = Raylib.MeasureText(livesText, 24);
        Raylib.DrawText(livesText, _scene.ScreenWidth - livesWidth - 32, 18, 24, BreakoutTheme.TextColor);

        string controlsText = "Move: A/D or arrows   Launch: Space";
        int controlsWidth = Raylib.MeasureText(controlsText, 18);
        Raylib.DrawText(controlsText, (_scene.ScreenWidth - controlsWidth) / 2, _scene.ScreenHeight - 28, 18, BreakoutTheme.MutedTextColor);
    }

    private void DrawOverlay()
    {
        if (_scene.Phase == GamePhase.Playing)
        {
            return;
        }

        string title;
        string subtitle;

        if (_scene.Phase == GamePhase.Serving)
        {
            title = "Breakout";
            subtitle = "Press Space to launch the ball";
        }
        else if (_scene.Phase == GamePhase.Won)
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
        int panelX = (_scene.ScreenWidth - panelWidth) / 2;
        int panelY = (_scene.ScreenHeight - panelHeight) / 2;

        Raylib.DrawRectangle(panelX, panelY, panelWidth, panelHeight, BreakoutTheme.OverlayColor);
        Raylib.DrawRectangleLines(panelX, panelY, panelWidth, panelHeight, BreakoutTheme.BorderColor);

        DrawCenteredText(title, panelY + 28, 32, BreakoutTheme.TextColor);
        DrawCenteredText(subtitle, panelY + 72, 20, BreakoutTheme.MutedTextColor);
    }

    private void DrawCenteredText(string text, int y, int fontSize, Color color)
    {
        int textWidth = Raylib.MeasureText(text, fontSize);
        Raylib.DrawText(text, (_scene.ScreenWidth - textWidth) / 2, y, fontSize, color);
    }
}
