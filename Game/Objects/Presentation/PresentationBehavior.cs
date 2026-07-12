using System.Numerics;
using Raylib_cs;

namespace Breakout;

sealed class PresentationBehavior(
    GameObject scoreText,
    GameObject livesText,
    GameObject controlsText,
    GameObject overlayPanel,
    GameObject overlayTitle,
    GameObject overlaySubtitle) : Component, IUpdatable
{
    public void Update(float dt)
    {
        BreakoutScene scene = (BreakoutScene)Scene;

        SetText(scoreText, $"Score: {scene.Score}");
        SetText(livesText, $"Lives: {scene.Lives}");
        AlignRight(livesText, scene.ScreenWidth - 32f);

        SetText(controlsText, "Move: A/D or arrows   Launch: Space");
        CenterText(controlsText, scene.ScreenWidth, scene.ScreenHeight - 28f);

        UpdateOverlay(scene);
    }

    private void UpdateOverlay(BreakoutScene scene)
    {
        bool visible = scene.Phase != GamePhase.Playing;
        SetVisible(overlayPanel, visible);
        SetVisible(overlayTitle, visible);
        SetVisible(overlaySubtitle, visible);

        if (!visible)
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

        SetText(overlayTitle, title);
        SetText(overlaySubtitle, subtitle);
        CenterText(overlayTitle, scene.ScreenWidth, overlayTitle.Transform2D.Position.Y);
        CenterText(overlaySubtitle, scene.ScreenWidth, overlaySubtitle.Transform2D.Position.Y);
    }

    private static void SetText(GameObject gameObject, string text)
    {
        TextRendererComponent renderer = gameObject.Get<TextRendererComponent>();
        renderer.Text = text;
    }

    private static void SetVisible(GameObject gameObject, bool visible)
    {
        gameObject.Visible = visible;
    }

    private static void CenterText(GameObject gameObject, int screenWidth, float y)
    {
        TextRendererComponent renderer = gameObject.Get<TextRendererComponent>();
        Transform2D transform = gameObject.Transform2D;
        int width = Raylib.MeasureText(renderer.Text, renderer.FontSize);
        transform.Position = new Vector2((screenWidth - width) * 0.5f, y);
        gameObject.Transform2D = transform;
    }

    private static void AlignRight(GameObject gameObject, float right)
    {
        TextRendererComponent renderer = gameObject.Get<TextRendererComponent>();
        Transform2D transform = gameObject.Transform2D;
        int width = Raylib.MeasureText(renderer.Text, renderer.FontSize);
        transform.Position = new Vector2(right - width, transform.Position.Y);
        gameObject.Transform2D = transform;
    }
}
