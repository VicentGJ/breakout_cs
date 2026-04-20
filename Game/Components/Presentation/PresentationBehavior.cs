using System.Numerics;
using Raylib_cs;

namespace Breakout;

sealed class PresentationBehavior : BehaviorComponent
{
    private readonly GameObject _scoreText;
    private readonly GameObject _livesText;
    private readonly GameObject _controlsText;
    private readonly GameObject _overlayPanel;
    private readonly GameObject _overlayTitle;
    private readonly GameObject _overlaySubtitle;

    public PresentationBehavior(
        GameObject scoreText,
        GameObject livesText,
        GameObject controlsText,
        GameObject overlayPanel,
        GameObject overlayTitle,
        GameObject overlaySubtitle)
    {
        _scoreText = scoreText;
        _livesText = livesText;
        _controlsText = controlsText;
        _overlayPanel = overlayPanel;
        _overlayTitle = overlayTitle;
        _overlaySubtitle = overlaySubtitle;
    }

    public override void Update(float dt)
    {
        BreakoutScene scene = (BreakoutScene)Scene;

        SetText(_scoreText, $"Score: {scene.Score}");
        SetText(_livesText, $"Lives: {scene.Lives}");
        AlignRight(_livesText, scene.ScreenWidth - 32f);

        SetText(_controlsText, "Move: A/D or arrows   Launch: Space");
        CenterText(_controlsText, scene.ScreenWidth, scene.ScreenHeight - 28f);

        UpdateOverlay(scene);
    }

    private void UpdateOverlay(BreakoutScene scene)
    {
        bool visible = scene.Phase != GamePhase.Playing;
        SetVisible(_overlayPanel, visible);
        SetVisible(_overlayTitle, visible);
        SetVisible(_overlaySubtitle, visible);

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

        SetText(_overlayTitle, title);
        SetText(_overlaySubtitle, subtitle);
        CenterText(_overlayTitle, scene.ScreenWidth, _overlayTitle.Transform2D.Position.Y);
        CenterText(_overlaySubtitle, scene.ScreenWidth, _overlaySubtitle.Transform2D.Position.Y);
    }

    private static void SetText(GameObject gameObject, string text)
    {
        TextRendererComponent renderer = gameObject.Get<TextRendererComponent>();
        renderer.Text = text;
        gameObject.Set(renderer);
    }

    private static void SetVisible(GameObject gameObject, bool visible)
    {
        CanvasItemComponent canvasItem = gameObject.Get<CanvasItemComponent>();
        canvasItem.Visible = visible;
        gameObject.Set(canvasItem);
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
