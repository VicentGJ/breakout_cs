using System.Numerics;
using Raylib_cs;

namespace Breakout;

static class PresentationObjects
{
    public static void Create(BreakoutScene scene)
    {
        GameObject arenaObject = CreateRectangleObject(
            scene,
            "Arena",
            new Vector2(scene.Arena.X, scene.Arena.Y),
            new Vector2(scene.Arena.Width, scene.Arena.Height),
            BreakoutTheme.ArenaColor,
            zIndex: 0);
        RectangleRendererComponent arenaRenderer = arenaObject.Get<RectangleRendererComponent>();
        arenaRenderer.StrokeColor = BreakoutTheme.BorderColor;
        arenaObject.Set(arenaRenderer);

        GameObject scoreText = CreateTextObject(scene, "ScoreText", new Vector2(32f, 18f), 24, BreakoutTheme.TextColor, zIndex: 100);
        GameObject livesText = CreateTextObject(scene, "LivesText", Vector2.Zero, 24, BreakoutTheme.TextColor, zIndex: 100);
        GameObject controlsText = CreateTextObject(scene, "ControlsText", Vector2.Zero, 18, BreakoutTheme.MutedTextColor, zIndex: 100);

        const int panelWidth = 420;
        const int panelHeight = 120;
        Vector2 panelPosition = new((scene.ScreenWidth - panelWidth) * 0.5f, (scene.ScreenHeight - panelHeight) * 0.5f);
        GameObject overlayPanel = CreateRectangleObject(scene, "OverlayPanel", panelPosition, new Vector2(panelWidth, panelHeight), BreakoutTheme.OverlayColor, zIndex: 200);
        RectangleRendererComponent overlayRenderer = overlayPanel.Get<RectangleRendererComponent>();
        overlayRenderer.StrokeColor = BreakoutTheme.BorderColor;
        overlayPanel.Set(overlayRenderer);

        GameObject overlayTitle = CreateTextObject(scene, "OverlayTitle", new Vector2(0f, panelPosition.Y + 28f), 32, BreakoutTheme.TextColor, zIndex: 201);
        GameObject overlaySubtitle = CreateTextObject(scene, "OverlaySubtitle", new Vector2(0f, panelPosition.Y + 72f), 20, BreakoutTheme.MutedTextColor, zIndex: 201);

        GameObject presentationState = new("PresentationState");
        presentationState.AddBehavior(new PresentationBehavior(scoreText, livesText, controlsText, overlayPanel, overlayTitle, overlaySubtitle));
        scene.AddObject(presentationState);
    }

    private static GameObject CreateRectangleObject(Scene scene, string name, Vector2 position, Vector2 size, Color fillColor, int zIndex)
    {
        GameObject gameObject = new(name);
        gameObject.Transform2D = new Transform2D { Position = position, Scale = Vector2.One };
        gameObject.ZIndex = zIndex;
        gameObject.Add(new RectangleRendererComponent { Size = size, FillColor = fillColor });
        scene.AddObject(gameObject);
        return gameObject;
    }

    private static GameObject CreateTextObject(Scene scene, string name, Vector2 position, int fontSize, Color color, int zIndex)
    {
        GameObject gameObject = new(name);
        gameObject.Transform2D = new Transform2D { Position = position, Scale = Vector2.One };
        gameObject.ZIndex = zIndex;
        gameObject.Add(new TextRendererComponent { FontSize = fontSize, Color = color });
        scene.AddObject(gameObject);
        return gameObject;
    }
}
