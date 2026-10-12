using System.Numerics;
using Raylib_cs;

namespace Breakout;

sealed class Hud : GameObject
{
    private const int PanelWidth = 420;
    private const int PanelHeight = 120;

    private readonly Label _scoreText;
    private readonly Label _livesText;
    private readonly Label _controlsText;
    private readonly RectangleShape _overlayPanel;
    private readonly Label _overlayTitle;
    private readonly Label _overlaySubtitle;
    private GameState _game = null!;

    public Hud()
    {
        _scoreText = AddChild(new Label { Name = "ScoreText", ZIndex = 100, FontSize = 24, Color = BreakoutTheme.TextColor });
        _livesText = AddChild(new Label { Name = "LivesText", ZIndex = 100, FontSize = 24, Color = BreakoutTheme.TextColor });
        _controlsText = AddChild(new Label { Name = "ControlsText", ZIndex = 100, FontSize = 18, Color = BreakoutTheme.MutedTextColor });
        _overlayPanel = AddChild(new RectangleShape
        {
            Name = "OverlayPanel",
            ZIndex = 200,
            Size = new Vector2(PanelWidth, PanelHeight),
            FillColor = BreakoutTheme.OverlayColor,
            StrokeColor = BreakoutTheme.BorderColor
        });
        _overlayTitle = AddChild(new Label { Name = "OverlayTitle", ZIndex = 201, FontSize = 32, Color = BreakoutTheme.TextColor });
        _overlaySubtitle = AddChild(new Label { Name = "OverlaySubtitle", ZIndex = 201, FontSize = 20, Color = BreakoutTheme.MutedTextColor });
    }

    public override void Start()
    {
        BreakoutWorld world = (BreakoutWorld)World;
        _game = World.Find<GameState>()!;

        Vector2 panelPosition = new((world.ScreenWidth - PanelWidth) * 0.5f, (world.ScreenHeight - PanelHeight) * 0.5f);
        SetPosition(_scoreText, new Vector2(32f, 18f));
        SetPosition(_overlayPanel, panelPosition);
        SetPosition(_overlayTitle, new Vector2(0f, panelPosition.Y + 28f));
        SetPosition(_overlaySubtitle, new Vector2(0f, panelPosition.Y + 72f));
    }

    public override void Update(float dt)
    {
        BreakoutWorld world = (BreakoutWorld)World;

        _scoreText.Text = $"Score: {_game.Score}";
        _livesText.Text = $"Lives: {_game.Lives}";
        AlignRight(_livesText, world.ScreenWidth - 32f);

        _controlsText.Text = "Move: A/D or arrows   Launch: Space";
        CenterText(_controlsText, world.ScreenWidth, world.ScreenHeight - 28f);

        UpdateOverlay(world);
    }

    private void UpdateOverlay(BreakoutWorld world)
    {
        bool visible = _game.Phase != GamePhase.Playing;
        _overlayPanel.Visible = visible;
        _overlayTitle.Visible = visible;
        _overlaySubtitle.Visible = visible;

        if (!visible)
        {
            return;
        }

        string title;
        string subtitle;

        if (_game.Phase == GamePhase.Serving)
        {
            title = "Breakout";
            subtitle = "Press Space to launch the ball";
        }
        else if (_game.Phase == GamePhase.Won)
        {
            title = "You cleared the wall";
            subtitle = "Press Space or Enter to play again";
        }
        else
        {
            title = "Game over";
            subtitle = "Press Space or Enter to try again";
        }

        _overlayTitle.Text = title;
        _overlaySubtitle.Text = subtitle;
        CenterText(_overlayTitle, world.ScreenWidth, _overlayTitle.Transform2D.Position.Y);
        CenterText(_overlaySubtitle, world.ScreenWidth, _overlaySubtitle.Transform2D.Position.Y);
    }

    private static void SetPosition(GameObject gameObject, Vector2 position)
    {
        Transform2D transform = gameObject.Transform2D;
        transform.Position = position;
        gameObject.Transform2D = transform;
    }

    private static void CenterText(Label label, int screenWidth, float y)
    {
        int width = Raylib.MeasureText(label.Text, label.FontSize);
        SetPosition(label, new Vector2((screenWidth - width) * 0.5f, y));
    }

    private static void AlignRight(Label label, float right)
    {
        int width = Raylib.MeasureText(label.Text, label.FontSize);
        SetPosition(label, new Vector2(right - width, label.Transform2D.Position.Y));
    }
}
